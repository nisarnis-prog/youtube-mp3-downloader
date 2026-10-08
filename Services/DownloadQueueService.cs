using System.Collections.ObjectModel;
using System.IO;
using YouTube_MP3_Downloader.Models;

namespace YouTube_MP3_Downloader.Services;

public sealed class DownloadQueueService : IDisposable
{
    private readonly object _lock = new();
    private readonly YtDlpService _ytDlp;
    private readonly LoggerService _logger;
    private readonly Func<string> _outputFolderProvider;
    private readonly Func<int> _bitrateProvider;

    private readonly Dictionary<string, CancellationTokenSource> _activeCancellations = new();
    private int _activeCount;
    private int _maxConcurrent = 3;
    private bool _paused;

    public ObservableCollection<DownloadItem> Items { get; } = new();

    public event EventHandler? CountsChanged;
    public event EventHandler? QueueStateChanged;

    public DownloadQueueService(
        YtDlpService ytDlp,
        LoggerService logger,
        Func<string> outputFolderProvider,
        Func<int> bitrateProvider)
    {
        _ytDlp = ytDlp;
        _logger = logger;
        _outputFolderProvider = outputFolderProvider;
        _bitrateProvider = bitrateProvider;
    }

    public bool IsPaused
    {
        get { lock (_lock) return _paused; }
    }

    public int MaxConcurrent
    {
        get { lock (_lock) return _maxConcurrent; }
        set
        {
            lock (_lock)
            {
                var previous = _maxConcurrent;
                _maxConcurrent = Math.Clamp(value, 1, 5);
                if (_maxConcurrent != previous)
                {
                    _logger.Info($"Concurrency limit changed from {previous} to {_maxConcurrent} (active={_activeCount})");
                }
            }

            TryStartNext();
        }
    }

    public void Pause()
    {
        lock (_lock)
        {
            if (_paused)
            {
                return;
            }

            _paused = true;
        }

        _logger.Info("Queue paused");
        QueueStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Resume()
    {
        lock (_lock)
        {
            if (!_paused)
            {
                return;
            }

            _paused = false;
        }

        _logger.Info("Queue resumed");
        QueueStateChanged?.Invoke(this, EventArgs.Empty);
        TryStartNext();
    }

    public (int Added, int Skipped) AddUrls(IEnumerable<string> urls, bool autoStart = true)
    {
        int added;
        int skipped;

        lock (_lock)
        {
            added = 0;
            skipped = 0;

            foreach (var rawUrl in urls)
            {
                var url = rawUrl.Trim();
                if (url.Length == 0)
                {
                    continue;
                }

                var isDuplicate = Items.Any(i => string.Equals(i.Url, url, StringComparison.OrdinalIgnoreCase));
                if (isDuplicate)
                {
                    skipped++;
                    continue;
                }

                Items.Add(new DownloadItem { Url = url });
                added++;
            }
        }

        if (added == 0)
        {
            RaiseCountsChanged();
            return (added, skipped);
        }

        RaiseCountsChanged();
        if (autoStart)
        {
            TryStartNext();
        }

        return (added, skipped);
    }

    public bool CancelItem(DownloadItem item)
    {
        lock (_lock)
        {
            if (item.Status == DownloadStatus.Waiting)
            {
                item.Status = DownloadStatus.Cancelled;
                RaiseCountsChanged();
                return true;
            }

            if (_activeCancellations.TryGetValue(item.Id, out var cts))
            {
                cts.Cancel();
                return true;
            }
        }

        return false;
    }

    public void CancelAll()
    {
        List<CancellationTokenSource> toCancel;
        lock (_lock)
        {
            toCancel = new List<CancellationTokenSource>(_activeCancellations.Values);
            foreach (var item in Items)
            {
                if (item.Status == DownloadStatus.Waiting)
                {
                    item.Status = DownloadStatus.Cancelled;
                }
            }
        }

        foreach (var cts in toCancel)
        {
            try
            {
                cts.Cancel();
            }
            catch
            {
            }
        }

        _logger.Info("Cancel all requested");
        RaiseCountsChanged();
    }

    public void ClearCompleted()
    {
        lock (_lock)
        {
            var completed = Items.Where(i => i.Status == DownloadStatus.Completed).ToList();
            foreach (var item in completed)
            {
                Items.Remove(item);
            }
        }

        RaiseCountsChanged();
    }

    public void RemoveItem(DownloadItem item)
    {
        lock (_lock)
        {
            if (_activeCancellations.TryGetValue(item.Id, out var cts))
            {
                cts.Cancel();
            }

            Items.Remove(item);
        }

        RaiseCountsChanged();
    }

    public void RetryItem(DownloadItem item)
    {
        lock (_lock)
        {
            if (item.Status != DownloadStatus.Failed && item.Status != DownloadStatus.Cancelled)
            {
                return;
            }

            item.Title = "";
            item.ErrorMessage = "";
            item.Speed = "";
            item.Eta = "";
            item.FileSize = "";
            item.Progress = 0;
            item.Status = DownloadStatus.Waiting;
        }

        _logger.Info($"Retry queued for {item.Url}");
        RaiseCountsChanged();
        TryStartNext();
    }

    public (int Active, int Waiting, int Completed, int Failed, int Total) GetCounts()
    {
        lock (_lock)
        {
            int active = 0, waiting = 0, completed = 0, failed = 0;
            foreach (var item in Items)
            {
                switch (item.Status)
                {
                    case DownloadStatus.Waiting:
                        waiting++;
                        break;
                    case DownloadStatus.Completed:
                        completed++;
                        break;
                    case DownloadStatus.Failed:
                        failed++;
                        break;
                    case DownloadStatus.Preparing:
                    case DownloadStatus.Downloading:
                    case DownloadStatus.Converting:
                        active++;
                        break;
                }
            }

            return (active, waiting, completed, failed, Items.Count);
        }
    }

    public void TryStartNext()
    {
        while (TryAcquireNext(out var item))
        {
            _ = ProcessItemAsync(item);
        }
    }

    private bool TryAcquireNext(out DownloadItem item)
    {
        item = null!;
        lock (_lock)
        {
            if (_paused || _activeCount >= _maxConcurrent)
            {
                return false;
            }

            var candidate = Items.FirstOrDefault(i => i.Status == DownloadStatus.Waiting);
            if (candidate is null)
            {
                return false;
            }

            _activeCount++;
            _activeCancellations[candidate.Id] = new CancellationTokenSource();
            candidate.Status = DownloadStatus.Preparing;
            item = candidate;
            return true;
        }
    }

    private async Task ProcessItemAsync(DownloadItem item)
    {
        CancellationTokenSource? cts;
        lock (_lock)
        {
            _activeCancellations.TryGetValue(item.Id, out cts);
        }

        var cancellation = cts?.Token ?? CancellationToken.None;
        var progress = new Progress<DownloadProgressUpdate>(update => ApplyUpdate(item, update));

        try
        {
            _logger.Info($"Download started: {item.Url}");
            var info = await _ytDlp.FetchInfoAsync(item.Url, cancellation).ConfigureAwait(false);
            item.Title = info.Title;
            item.Status = DownloadStatus.Downloading;

            if (cancellation.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellation);
            }

            var outputPath = BuildUniqueOutputPath(_outputFolderProvider(), info.Title, "mp3");

            await _ytDlp.DownloadAsync(item.Url, outputPath, _bitrateProvider(), cancellation, progress).ConfigureAwait(false);

            item.OutputFilePath = outputPath;
            item.Progress = 100;
            item.Status = DownloadStatus.Completed;
            _logger.Info($"Download completed: {item.Url} -> {outputPath}");
        }
        catch (OperationCanceledException)
        {
            item.Status = DownloadStatus.Cancelled;
            item.Speed = "";
            item.Eta = "";
            _logger.Info($"Download cancelled: {item.Url}");
        }
        catch (Exception ex)
        {
            item.Progress = 0;
            item.Status = DownloadStatus.Failed;
            item.ErrorMessage = ex.Message;
            _logger.Error($"Download failed: {item.Url}", ex);
        }
        finally
        {
            lock (_lock)
            {
                _activeCount--;
                _activeCancellations.Remove(item.Id);
            }

            RaiseCountsChanged();
            TryStartNext();
        }
    }

    private void ApplyUpdate(DownloadItem item, DownloadProgressUpdate update)
    {
        if (update.Percent is not null)
        {
            item.Progress = Math.Clamp(update.Percent.Value, 0, 100);
        }

        if (update.Speed is not null)
        {
            item.Speed = update.Speed;
        }

        if (update.Eta is not null)
        {
            item.Eta = update.Eta;
        }

        if (update.Size is not null)
        {
            item.FileSize = update.Size;
        }

        if (update.Status is not null)
        {
            item.Status = update.Status.Value;
        }
    }

    private void RaiseCountsChanged()
    {
        CountsChanged?.Invoke(this, EventArgs.Empty);
    }

    public static string BuildUniqueOutputPath(string folder, string title, string extension)
    {
        var safeBase = SanitizeFileName(title);
        if (safeBase.Length == 0)
        {
            safeBase = "audio";
        }

        var candidate = Path.Combine(folder, $"{safeBase}.{extension}");
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        for (var i = 1; i < 1000; i++)
        {
            candidate = Path.Combine(folder, $"{safeBase} ({i}).{extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        return Path.Combine(folder, $"{safeBase}_{Guid.NewGuid():N}.{extension}");
    }

    public static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.ToCharArray();
        var sanitized = new List<char>(chars.Length);

        foreach (var c in chars)
        {
            if (Array.IndexOf(invalid, c) >= 0)
            {
                sanitized.Add('_');
            }
            else if (c < 32)
            {
                sanitized.Add('_');
            }
            else
            {
                sanitized.Add(c);
            }
        }

        var result = new string(sanitized.ToArray()).Trim().TrimEnd('.', ' ');
        return result.Length > 180 ? result[..180] : result;
    }

    public void Dispose()
    {
        List<CancellationTokenSource> toCancel;
        lock (_lock)
        {
            toCancel = new List<CancellationTokenSource>(_activeCancellations.Values);
            _activeCancellations.Clear();
        }

        foreach (var cts in toCancel)
        {
            try
            {
                cts.Cancel();
            }
            catch
            {
            }
        }
    }
}