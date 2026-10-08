using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using YouTube_MP3_Downloader.Models;
using YouTube_MP3_Downloader.Services;

namespace YouTube_MP3_Downloader.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly DownloadQueueService _queue;
    private readonly LoggerService _logger;
    private readonly PlaybackService _playback;
    private readonly DispatcherTimer _playbackTimer;

    private string _bulkInput = "";
    private string _statusMessage = "";
    private Mp3Option? _selectedMp3Option;
    private BrowserCookieOption? _selectedCookieBrowser;
    private CookieMethodOption? _selectedCookieMethod;
    private bool _useBrowserCookies;
    private bool _useCookieFile;
    private string _cookieFilePath = "";
    private int _activeCount;
    private int _waitingCount;
    private int _completedCount;
    private int _failedCount;
    private int _currentPageIndex;
    private bool _isDarkTheme = true;
    private double _totalProgress;
    private DownloadItem? _nowPlayingItem;
    private DownloadItem? _currentRow;
    private string _nowPlayingTitle = "";
    private string _nowPlayingPath = "";
    private TimeSpan _currentPosition;
    private TimeSpan _duration;
    private bool _hasDuration;
    private double _durationSeconds;
    private double _playbackProgress;
    private bool _isSeeking;
    private string _positionText = "0:00";
    private string _durationText = "--:--";
    private string _localFolderPath = "";
    private LocalTrack? _selectedLocalTrack;
    private bool _isPlaying;
    private bool _isPaused;
    private string _playbackError = "";

    public event EventHandler<bool>? ThemeSwitchRequested;

    public MainViewModel(
        SettingsService settingsService,
        DownloadQueueService queue,
        LoggerService logger)
    {
        _settingsService = settingsService;
        _queue = queue;
        _logger = logger;
        _settings = settingsService.Load();

        _isDarkTheme = !string.Equals(_settings.Theme, "Light", StringComparison.OrdinalIgnoreCase);
        _selectedMp3Option = BitrateOptions.FirstOrDefault(o => o.Kbps == _settings.Mp3Bitrate) ?? BitrateOptions[^1];
        _useBrowserCookies = _settings.UseBrowserCookies;
        _useCookieFile = _settings.UseCookieFile;
        _cookieFilePath = _settings.CookieFilePath ?? "";
        _selectedCookieBrowser = CookieBrowserOptions.FirstOrDefault(o => o.Value == _settings.BrowserCookieSource) ?? CookieBrowserOptions[0];
        _selectedCookieMethod = CookieMethodOptions.FirstOrDefault(o => o.UsesFile == _settings.UseCookieFile) ?? CookieMethodOptions[0];
        OutputFolder = _settings.OutputFolder;
        _queue.MaxConcurrent = _settings.MaxConcurrentDownloads;
        _logger.Info($"Download concurrency initialized from settings: {_settings.MaxConcurrentDownloads}");

        _playback = new PlaybackService(_logger);
        _playback.StateChanged += (_, _) => OnPlaybackStateChanged();
        _playback.Failure += (_, reason) => PlaybackError = reason;

        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _playbackTimer.Tick += (_, _) => UpdatePlaybackProgress();

        _queue.CountsChanged += (_, _) => RefreshCounters();
        _queue.QueueStateChanged += (_, _) =>
        {
            RefreshCounters();
            InvalidateCommands();
        };

        AddUrlsCommand = new AsyncRelayCommand(AddUrlsAsync);
        ClearInputCommand = new RelayCommand(() => BulkInput = "");
        BrowseFolderCommand = new RelayCommand(BrowseOutputFolder);
        OpenFolderCommand = new RelayCommand(OpenOutputFolder);
        BrowseCookieFileCommand = new RelayCommand(BrowseCookieFile);
        StartQueueCommand = new RelayCommand(StartQueue);
        PauseQueueCommand = new RelayCommand(PauseQueue, () => !IsPaused);
        ResumeQueueCommand = new RelayCommand(ResumeQueue, () => IsPaused);
        CancelAllCommand = new RelayCommand(CancelAll);
        ClearCompletedCommand = new RelayCommand(ClearCompleted, () => CompletedCount > 0);
        CancelItemCommand = new RelayCommand(i => { if (i is DownloadItem item) { _queue.CancelItem(item); RefreshCounters(); } });
        RetryItemCommand = new RelayCommand(i => { if (i is DownloadItem item) { StopPlaybackIfNowPlaying(item); _queue.RetryItem(item); RefreshCounters(); } });
        RemoveItemCommand = new RelayCommand(i =>
        {
            if (i is DownloadItem item)
            {
                StopPlaybackIfNowPlaying(item);
                _queue.RemoveItem(item);
                RefreshCounters();
            }
        });
        PlayItemCommand = new RelayCommand(i => { if (i is DownloadItem item) { PlayItem(item); } });
        TogglePlayItemCommand = new RelayCommand(i => { if (i is DownloadItem item) { TogglePlayItem(item); } });
        PlayPauseCommand = new RelayCommand(PlayPause, () => HasPlayableNowPlaying);
        StopPlaybackCommand = new RelayCommand(StopPlayback, () => HasNowPlaying);
        OpenDownloadedFileCommand = new RelayCommand(OpenDownloadedFile, () => HasPlayableNowPlaying);
        SelectMp3Command = new RelayCommand(SelectMp3);
        SelectMusicFolderCommand = new RelayCommand(SelectMusicFolder);
        PlayLocalTrackCommand = new RelayCommand(p => { if (p is LocalTrack track) { PlayLocalTrack(track); } });
    }

    public ObservableCollection<DownloadItem> Items => _queue.Items;

    public IReadOnlyList<int> ConcurrencyOptions { get; } = new[] { 1, 2, 3, 4, 5 };

    public IReadOnlyList<Mp3Option> BitrateOptions { get; } = new[]
    {
        new Mp3Option(128),
        new Mp3Option(192),
        new Mp3Option(256),
        new Mp3Option(320)
    };

    public IReadOnlyList<BrowserCookieOption> CookieBrowserOptions { get; } = new[]
    {
        new BrowserCookieOption("Chrome", "chrome"),
        new BrowserCookieOption("Edge", "edge"),
        new BrowserCookieOption("Firefox", "firefox")
    };

    public IReadOnlyList<CookieMethodOption> CookieMethodOptions { get; } = new[]
    {
        new CookieMethodOption("Browser cookies", false),
        new CookieMethodOption("Cookies file", true)
    };

    public ICommand AddUrlsCommand { get; }
    public ICommand ClearInputCommand { get; }
    public ICommand BrowseFolderCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand BrowseCookieFileCommand { get; }
    public ICommand StartQueueCommand { get; }
    public ICommand PauseQueueCommand { get; }
    public ICommand ResumeQueueCommand { get; }
    public ICommand CancelAllCommand { get; }
    public ICommand ClearCompletedCommand { get; }
    public ICommand CancelItemCommand { get; }
    public ICommand RetryItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand PlayItemCommand { get; }
    public ICommand TogglePlayItemCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand StopPlaybackCommand { get; }
    public ICommand OpenDownloadedFileCommand { get; }
    public ICommand SelectMp3Command { get; }
    public ICommand SelectMusicFolderCommand { get; }
    public ICommand PlayLocalTrackCommand { get; }

    public string BulkInput
    {
        get => _bulkInput;
        set => SetField(ref _bulkInput, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public IReadOnlyList<string> NavItems { get; } = new[] { "Home", "Downloads", "Settings", "About" };

    public double TotalProgress
    {
        get => _totalProgress;
        private set => SetField(ref _totalProgress, value);
    }

    public DownloadItem? NowPlayingItem
    {
        get => _nowPlayingItem;
        private set => SetField(ref _nowPlayingItem, value);
    }

    public string NowPlayingTitle => _nowPlayingTitle.Length == 0 ? "No track selected" : _nowPlayingTitle;

    public string NowPlayingPath => _nowPlayingPath;

    public bool HasNowPlaying => _nowPlayingPath.Length > 0;

    public bool IsMediaPlaying
    {
        get => _isPlaying;
        private set => SetField(ref _isPlaying, value);
    }

    public bool IsMediaPaused
    {
        get => _isPaused;
        private set => SetField(ref _isPaused, value);
    }

    public string PlaybackError
    {
        get => _playbackError;
        private set
        {
            if (SetField(ref _playbackError, value))
            {
                OnPropertyChanged(nameof(HasPlaybackError));
            }
        }
    }

    public bool HasPlaybackError => _playbackError.Length > 0;

    public bool HasPlayableNowPlaying => _nowPlayingPath.Length > 0 && File.Exists(_nowPlayingPath);

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        private set => SetField(ref _currentPosition, value);
    }

    public TimeSpan Duration
    {
        get => _duration;
        private set => SetField(ref _duration, value);
    }

    public bool HasDuration
    {
        get => _hasDuration;
        private set => SetField(ref _hasDuration, value);
    }

    public double DurationSeconds
    {
        get => _durationSeconds;
        private set => SetField(ref _durationSeconds, value);
    }

    public double PlaybackProgress
    {
        get => _playbackProgress;
        set => SetField(ref _playbackProgress, value);
    }

    public bool IsSeeking
    {
        get => _isSeeking;
        set => SetField(ref _isSeeking, value);
    }

    public string PositionText
    {
        get => _positionText;
        private set => SetField(ref _positionText, value);
    }

    public string DurationText
    {
        get => _durationText;
        private set => SetField(ref _durationText, value);
    }

    public ObservableCollection<LocalTrack> LocalTracks { get; } = new();

    public string LocalFolderPath
    {
        get => _localFolderPath;
        private set => SetField(ref _localFolderPath, value);
    }

    public string LocalFolderInfo => _localFolderPath.Length == 0
        ? ""
        : $"{_localFolderPath}  ({LocalTracks.Count} MP3{(LocalTracks.Count == 1 ? "" : "s")})";

    public LocalTrack? SelectedLocalTrack
    {
        get => _selectedLocalTrack;
        set => SetField(ref _selectedLocalTrack, value);
    }

    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        set => SetField(ref _currentPageIndex, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (!SetField(ref _isDarkTheme, value))
            {
                return;
            }

            _settings.Theme = value ? "Dark" : "Light";
            _settingsService.Save(_settings);
            OnPropertyChanged(nameof(ThemeName));
            ThemeSwitchRequested?.Invoke(this, value);
        }
    }

    public string ThemeName => IsDarkTheme ? "Dark theme" : "Light theme";

    public int MaxConcurrent
    {
        get => _settings.MaxConcurrentDownloads;
        set
        {
            if (value < 1 || value > 5 || value == _settings.MaxConcurrentDownloads)
            {
                return;
            }

            _settings.MaxConcurrentDownloads = value;
            _queue.MaxConcurrent = value;
            _settingsService.Save(_settings);
            OnPropertyChanged();
            _logger.Info($"Maximum concurrent downloads set to {value}");
            InvalidateCommands();
        }
    }

    public Mp3Option? SelectedMp3Option
    {
        get => _selectedMp3Option;
        set
        {
            if (value is null || value.Kbps == _settings.Mp3Bitrate)
            {
                return;
            }

            _settings.Mp3Bitrate = value.Kbps;
            _settingsService.Save(_settings);
            if (SetField(ref _selectedMp3Option, value))
            {
                OnPropertyChanged();
            }

            _logger.Info($"MP3 quality set to {value.Kbps} kbps");
        }
    }

    public bool UseBrowserCookies
    {
        get => _useBrowserCookies;
        set
        {
            if (!SetField(ref _useBrowserCookies, value))
            {
                return;
            }

            _settings.UseBrowserCookies = value;
            _settingsService.Save(_settings);
            _logger.Info(value ? "Authentication for restricted videos enabled" : "Authentication for restricted videos disabled");
            OnPropertyChanged(nameof(CookieAuthPlan));
        }
    }

    public bool UseCookieFile
    {
        get => _useCookieFile;
        private set
        {
            if (!SetField(ref _useCookieFile, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsCookieFileMode));
            OnPropertyChanged(nameof(CookieAuthPlan));
        }
    }

    public bool IsCookieFileMode => _useCookieFile;

    public CookieMethodOption? SelectedCookieMethod
    {
        get => _selectedCookieMethod;
        set
        {
            if (value is null || value.UsesFile == _useCookieFile)
            {
                return;
            }

            _settings.UseCookieFile = value.UsesFile;
            _settingsService.Save(_settings);
            if (SetField(ref _selectedCookieMethod, value))
            {
                OnPropertyChanged();
            }

            UseCookieFile = value.UsesFile;
            _logger.Info(value.UsesFile ? "Cookie authentication method set to cookies file" : "Cookie authentication method set to browser cookies");
        }
    }

    public string CookieFilePath
    {
        get => _cookieFilePath;
        set
        {
            var path = value?.Trim() ?? "";
            if (path.Equals(_cookieFilePath, StringComparison.Ordinal))
            {
                return;
            }

            _settings.CookieFilePath = path;
            _settingsService.Save(_settings);
            if (SetField(ref _cookieFilePath, path))
            {
                OnPropertyChanged();
            }

            OnPropertyChanged(nameof(CookieAuthPlan));
        }
    }

    public CookieAuthPlan? CookieAuthPlan
    {
        get
        {
            if (!_useBrowserCookies)
            {
                return null;
            }

            if (_useCookieFile)
            {
                var path = _cookieFilePath.Trim();
                return path.Length == 0
                    ? null
                    : new CookieAuthPlan(CookieAuthKind.File, path);
            }

            var source = _settings.BrowserCookieSource;
            if (string.IsNullOrWhiteSpace(source) || !IsAllowedBrowserSource(source))
            {
                return null;
            }

            return new CookieAuthPlan(CookieAuthKind.Browser, source);
        }
    }

    private static bool IsAllowedBrowserSource(string value)
    {
        return value.Equals("chrome", StringComparison.OrdinalIgnoreCase)
               || value.Equals("edge", StringComparison.OrdinalIgnoreCase)
               || value.Equals("firefox", StringComparison.OrdinalIgnoreCase);
    }

    public BrowserCookieOption? SelectedCookieBrowser
    {
        get => _selectedCookieBrowser;
        set
        {
            if (value is null || value.Value.Equals(_settings.BrowserCookieSource, StringComparison.Ordinal))
            {
                return;
            }

            _settings.BrowserCookieSource = value.Value;
            _settingsService.Save(_settings);
            if (SetField(ref _selectedCookieBrowser, value))
            {
                OnPropertyChanged();
            }

            _logger.Info($"Browser cookie source set to {value.Value}");
            OnPropertyChanged(nameof(CookieAuthPlan));
        }
    }

    public string OutputFolder
    {
        get => _settings.OutputFolder;
        set
        {
            var path = value?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(path) || path.Equals(_settings.OutputFolder, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not create output folder '{path}'", ex);
                StatusMessage = $"Could not create output folder: {ex.Message}";
                return;
            }

            _settings.OutputFolder = path;
            _settingsService.Save(_settings);
            OnPropertyChanged();
        }
    }

    public bool IsPaused => _queue.IsPaused;

    public int ActiveCount
    {
        get => _activeCount;
        private set => SetField(ref _activeCount, value);
    }

    public int WaitingCount
    {
        get => _waitingCount;
        private set => SetField(ref _waitingCount, value);
    }

    public int CompletedCount
    {
        get => _completedCount;
        private set => SetField(ref _completedCount, value);
    }

    public int FailedCount
    {
        get => _failedCount;
        private set => SetField(ref _failedCount, value);
    }

    public async Task AddUrlsAsync()
    {
        var raw = BulkInput ?? "";
        var candidates = new List<string>();
        var invalid = new List<string>();

        foreach (var line in raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var url = line.Trim();
            if (url.Length == 0)
            {
                continue;
            }

            if (IsSupportedYouTubeUrl(url))
            {
                candidates.Add(url);
            }
            else
            {
                invalid.Add(url);
            }
        }

        if (candidates.Count == 0)
        {
            StatusMessage = invalid.Count > 0
                ? "No valid YouTube links found. Only ordinary youtube.com / youtu.be links are accepted."
                : "Please paste at least one YouTube link.";
            return;
        }

        var (added, skipped) = _queue.AddUrls(candidates);
        foreach (var url in candidates)
        {
            _logger.Info($"URL added: {url}");
        }

        var parts = new List<string>();
        if (added > 0)
        {
            parts.Add($"{added} link{(added == 1 ? "" : "s")} added");
        }

        if (skipped > 0)
        {
            parts.Add($"{skipped} duplicate{(skipped == 1 ? "" : "s")} skipped");
        }

        if (invalid.Count > 0)
        {
            parts.Add($"{invalid.Count} invalid link{(invalid.Count == 1 ? "" : "s")} rejected");
        }

        StatusMessage = string.Join(", ", parts) + ".";
        BulkInput = "";
        await Task.CompletedTask;
    }

    public async Task<IpcSubmitResult> SubmitExternalUrlAsync(string url, bool autoStart)
    {
        var trimmed = url?.Trim() ?? "";
        if (!IsSupportedYouTubeUrl(trimmed))
        {
            _logger.Warn($"Rejected URL from browser companion: {trimmed}");
            return new IpcSubmitResult(false, "Unsupported YouTube URL.", 0, 0);
        }

        var (added, skipped) = _queue.AddUrls(new[] { trimmed }, autoStart);
        if (autoStart && _queue.IsPaused)
        {
            _queue.Resume();
        }

        if (added > 0)
        {
            _logger.Info($"URL added from browser companion: {trimmed}");
            StatusMessage = "Link added to the queue from the browser extension.";
        }
        else if (skipped > 0)
        {
            _logger.Info($"Duplicate URL skipped from browser companion: {trimmed}");
            StatusMessage = "Link is already in the queue.";
        }
        else
        {
            StatusMessage = "No changes.";
        }

        RefreshCounters();
        InvalidateCommands();
        return added > 0
            ? new IpcSubmitResult(true, "Link sent to downloader.", added, skipped)
            : new IpcSubmitResult(true, "Link is already in the download queue.", 0, skipped);
    }

    private void StartQueue()
    {
        if (IsPaused)
        {
            _queue.Resume();
        }

        _queue.TryStartNext();
        StatusMessage = "Queue started.";
        InvalidateCommands();
    }

    private void PauseQueue()
    {
        _queue.Pause();
        StatusMessage = "Queue paused. Running downloads will finish; waiting items will not start.";
        InvalidateCommands();
    }

    private void ResumeQueue()
    {
        _queue.Resume();
        StatusMessage = "Queue resumed.";
        InvalidateCommands();
    }

    private void CancelAll()
    {
        _queue.CancelAll();
        StatusMessage = "All downloads cancelled.";
        RefreshCounters();
        InvalidateCommands();
    }

    private void ClearCompleted()
    {
        _queue.ClearCompleted();
        if (_nowPlayingItem is { Status: DownloadStatus.Completed })
        {
            _playback.Stop();
            ClearPlaybackError();
            ClearNowPlaying();
        }

        StatusMessage = "Completed items cleared.";
        RefreshCounters();
        InvalidateCommands();
    }

    public void ShutdownPlayback()
    {
        _playback.Dispose();
    }

    private void PlayItem(DownloadItem item)
    {
        if (!File.Exists(item.OutputFilePath))
        {
            PlaybackError = "The MP3 file is no longer available on disk.";
            _logger.Warn($"Playback skipped: output file missing ({Path.GetFileName(item.OutputFilePath)})");
            return;
        }

        SetNowPlaying(item, item.Title, item.OutputFilePath);
        ClearPlaybackError();
        _playback.Play(item.OutputFilePath);
    }

    private void TogglePlayItem(DownloadItem item)
    {
        if (ReferenceEquals(_nowPlayingItem, item))
        {
            PlayPause();
        }
        else
        {
            PlayItem(item);
        }
    }

    private void PlayLocalTrack(LocalTrack track)
    {
        if (!File.Exists(track.FilePath))
        {
            PlaybackError = "The MP3 file is no longer available on disk.";
            _logger.Warn($"Playback skipped: local file missing ({track.FileName})");
            return;
        }

        SetNowPlaying(null, track.Title, track.FilePath);
        ClearPlaybackError();
        _playback.Play(track.FilePath);
    }

    private void PlayPause()
    {
        var path = _nowPlayingPath;
        if (path.Length == 0)
        {
            return;
        }

        if (!HasPlayableNowPlaying)
        {
            PlaybackError = "The MP3 file is no longer available on disk.";
            _logger.Warn($"Playback skipped: file missing ({Path.GetFileName(path)})");
            return;
        }

        switch (_playback.State)
        {
            case PlaybackState.Playing:
                _playback.Pause();
                break;
            case PlaybackState.Paused:
                _playback.Resume();
                break;
            default:
                ClearPlaybackError();
                _playback.Play(path);
                break;
        }
    }

    private void StopPlayback()
    {
        ClearPlaybackError();
        _playback.Stop();
    }

    private void OpenDownloadedFile()
    {
        var path = _nowPlayingPath;
        if (path.Length == 0 || !File.Exists(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
            _logger.Info($"Opened folder for file: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to open downloaded file folder", ex);
        }
    }

    private void SelectMp3()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an MP3 file",
            Filter = "MP3 files (*.mp3)|*.mp3",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var path = dialog.FileName.Trim();
        if (!File.Exists(path))
        {
            return;
        }

        PlayLocalTrack(new LocalTrack(path));
        _logger.Info($"MP3 selected for playback: {Path.GetFileName(path)}");
    }

    private void SelectMusicFolder()
    {
        string initialDirectory;
        try
        {
            initialDirectory = Directory.Exists(_localFolderPath)
                ? _localFolderPath
                : Directory.Exists(OutputFolder)
                    ? OutputFolder
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }
        catch
        {
            initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }

        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder containing MP3 files",
            InitialDirectory = initialDirectory
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        LoadLocalFolder(dialog.FolderName);
    }

    private void LoadLocalFolder(string folder)
    {
        LocalTracks.Clear();
        _selectedLocalTrack = null;
        LocalFolderPath = folder;

        try
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.mp3", SearchOption.TopDirectoryOnly)
                         .OrderBy(f => f, StringComparer.CurrentCultureIgnoreCase))
            {
                LocalTracks.Add(new LocalTrack(file));
            }
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not enumerate MP3 files in '{folder}': {ex.Message}");
            LocalFolderPath = "";
            OnPropertyChanged(nameof(LocalFolderInfo));
            return;
        }

        OnPropertyChanged(nameof(LocalFolderInfo));
        _logger.Info($"Loaded {LocalTracks.Count} MP3 file(s) from {folder}");
        if (LocalTracks.Count == 0)
        {
            StatusMessage = "No MP3 files found in the selected folder.";
        }
    }

    private void StopPlaybackIfNowPlaying(DownloadItem item)
    {
        if (!ReferenceEquals(_nowPlayingItem, item))
        {
            return;
        }

        _playback.Stop();
        ClearPlaybackError();
        ClearNowPlaying();
    }

    private void SetNowPlaying(DownloadItem? item, string title, string path)
    {
        _nowPlayingItem = item;
        _nowPlayingTitle = title;
        _nowPlayingPath = path;
        ResetPlaybackPosition();
        OnPropertyChanged(nameof(NowPlayingItem));
        OnPropertyChanged(nameof(NowPlayingTitle));
        OnPropertyChanged(nameof(NowPlayingPath));
        OnPropertyChanged(nameof(HasNowPlaying));
        InvalidateCommands();
        UpdateRowPlayState();
    }

    private void ClearNowPlaying()
    {
        _nowPlayingItem = null;
        _nowPlayingTitle = "";
        _nowPlayingPath = "";
        HasDuration = false;
        Duration = TimeSpan.Zero;
        DurationSeconds = 0;
        ResetPlaybackPosition();
        OnPropertyChanged(nameof(NowPlayingItem));
        OnPropertyChanged(nameof(NowPlayingTitle));
        OnPropertyChanged(nameof(NowPlayingPath));
        OnPropertyChanged(nameof(HasNowPlaying));
        InvalidateCommands();
        UpdateRowPlayState();
    }

    private void UpdateRowPlayState()
    {
        var item = _nowPlayingItem;
        if (!ReferenceEquals(_currentRow, item))
        {
            if (_currentRow is not null)
            {
                _currentRow.IsCurrentTrack = false;
                _currentRow.IsCurrentTrackPlaying = false;
            }

            if (item is not null)
            {
                item.IsCurrentTrack = true;
            }

            _currentRow = item;
        }

        if (item is not null)
        {
            item.IsCurrentTrackPlaying = _isPlaying;
        }
    }

    private void ResetPlaybackPosition()
    {
        CurrentPosition = TimeSpan.Zero;
        PlaybackProgress = 0;
        DurationSeconds = HasDuration ? Duration.TotalSeconds : 0;
        PositionText = FormatTime(TimeSpan.Zero);
        DurationText = HasDuration ? FormatTime(Duration) : "--:--";
    }

    private void UpdatePlaybackProgress()
    {
        if (_playback.State == PlaybackState.Stopped)
        {
            return;
        }

        HasDuration = _playback.HasDuration;
        Duration = _playback.Duration;
        DurationSeconds = _playback.HasDuration ? _playback.Duration.TotalSeconds : 0;

        if (IsSeeking)
        {
            PositionText = FormatTime(TimeSpan.FromSeconds(PlaybackProgress));
            return;
        }

        var position = _playback.CurrentPosition;
        CurrentPosition = position;
        PlaybackProgress = _playback.HasDuration
            ? Math.Clamp(position.TotalSeconds, 0, DurationSeconds)
            : 0;
        UpdateTimeTexts();
    }

    private void UpdateTimeTexts()
    {
        PositionText = FormatTime(CurrentPosition);
        DurationText = HasDuration ? FormatTime(Duration) : "--:--";
    }

    public void SeekToSeconds(double seconds)
    {
        IsSeeking = false;
        if (seconds < 0)
        {
            seconds = 0;
        }

        if (HasDuration && seconds > DurationSeconds)
        {
            seconds = DurationSeconds;
        }

        var target = TimeSpan.FromSeconds(seconds);
        _playback.Seek(target);

        CurrentPosition = target;
        PlaybackProgress = target.TotalSeconds;
        UpdateTimeTexts();
    }

    public void BeginSeek()
    {
        if (IsSeeking)
        {
            return;
        }

        IsSeeking = true;
        _logger.Info("Seek drag started");
    }

    public void EndSeek(double seconds)
    {
        _logger.Info($"Seek drag completed target={seconds:F1}s");
        SeekToSeconds(seconds);
    }

    public void ClickSeek(double seconds)
    {
        _logger.Info($"Seek click target={seconds:F1}s");
        SeekToSeconds(seconds);
    }

    private static string FormatTime(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        var totalSeconds = (long)value.TotalSeconds;
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds % 3600 / 60;
        var seconds = totalSeconds % 60;
        return hours > 0
            ? $"{hours}:{minutes:D2}:{seconds:D2}"
            : $"{minutes:D2}:{seconds:D2}";
    }

    private void ClearPlaybackError()
    {
        PlaybackError = "";
    }

    private void OnPlaybackStateChanged()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.InvokeAsync(OnPlaybackStateChanged);
            return;
        }

        IsMediaPlaying = _playback.State == PlaybackState.Playing;
        IsMediaPaused = _playback.State == PlaybackState.Paused;

        if (_playback.State == PlaybackState.Stopped)
        {
            if (_playbackTimer.IsEnabled)
            {
                _playbackTimer.Stop();
            }

            ResetPlaybackPosition();
        }
        else if (!_playbackTimer.IsEnabled)
        {
            _playbackTimer.Start();
        }

        UpdateRowPlayState();
    }

    private void BrowseOutputFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select the output folder for MP3 files",
            InitialDirectory = Directory.Exists(OutputFolder)
                ? OutputFolder
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFolder = dialog.FolderName;
            _logger.Info($"Output folder changed to: {dialog.FolderName}");
        }
    }

    private void BrowseCookieFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a cookies.txt file in Netscape format",
            Filter = "Cookie files (*.txt)|*.txt|All files (*.*)|*.*",
            CheckFileExists = true
        };

        var current = _settings.CookieFilePath;
        if (!string.IsNullOrWhiteSpace(current))
        {
            try
            {
                dialog.InitialDirectory = Path.GetDirectoryName(current) ?? "";
            }
            catch
            {
            }
        }

        if (dialog.ShowDialog() == true)
        {
            CookieFilePath = dialog.FileName.Trim();
            _logger.Info("Cookies file selected for authentication");
        }
    }

    private void OpenOutputFolder()
    {
        var folder = OutputFolder;
        if (!Directory.Exists(folder))
        {
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not create output folder '{folder}'", ex);
                StatusMessage = "Could not create output folder: " + ex.Message;
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to open output folder", ex);
        }
    }

    public void RefreshCounters()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.InvokeAsync(RefreshCountersCore);
            return;
        }

        RefreshCountersCore();
    }

    private void RefreshCountersCore()
    {
        var (active, waiting, completed, failed, total) = _queue.GetCounts();
        ActiveCount = active;
        WaitingCount = waiting;
        CompletedCount = completed;
        FailedCount = failed;
        TotalProgress = total > 0 ? Items.Average(i => i.Progress) : 0;
        InvalidateCommands();
    }

    private void InvalidateCommands()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.InvokeAsync(() => CommandManager.InvalidateRequerySuggested());
        }
        else
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private static bool IsSupportedYouTubeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var scheme = uri.Scheme;
        if (!string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var host = uri.Host.ToLowerInvariant();
        return host == "youtu.be"
               || host == "youtube.com"
               || host.EndsWith(".youtube.com", StringComparison.Ordinal);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}