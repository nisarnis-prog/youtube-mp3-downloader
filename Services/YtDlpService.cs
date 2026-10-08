using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using YouTube_MP3_Downloader.Models;

namespace YouTube_MP3_Downloader.Services;

public sealed record VideoInfo(string Title);

public sealed record DownloadProgressUpdate(double? Percent, string? Speed, string? Eta, string? Size, DownloadStatus? Status);

public sealed class YtDlpService
{
    private static readonly HashSet<string> AllowedCookieBrowsers = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome",
        "edge",
        "firefox"
    };

    private readonly LoggerService _logger;
    private readonly Func<CookieAuthPlan?> _cookieAuthProvider;
    private readonly object _errorLock = new();

    public string? YtDlpPath { get; }
    public string? FfmpegPath { get; }
    public string? FfprobePath { get; }

    public YtDlpService(LoggerService logger, Func<CookieAuthPlan?>? cookieAuthProvider = null)
    {
        _logger = logger;
        _cookieAuthProvider = cookieAuthProvider ?? (() => null);
        var toolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
        YtDlpPath = FindExisting(toolsDir, "yt-dlp.exe");
        FfmpegPath = FindExisting(toolsDir, "ffmpeg.exe");
        FfprobePath = FindExisting(toolsDir, "ffprobe.exe");
    }

    private static string? FindExisting(string dir, string fileName)
    {
        try
        {
            var path = Path.Combine(dir, fileName);
            return File.Exists(path) ? path : null;
        }
        catch
        {
            return null;
        }
    }

    public IReadOnlyList<string> GetMissingTools()
    {
        var missing = new List<string>();
        if (YtDlpPath is null) missing.Add("yt-dlp.exe");
        if (FfmpegPath is null) missing.Add("ffmpeg.exe");
        if (FfprobePath is null) missing.Add("ffprobe.exe");
        return missing;
    }

    private void EnsureTools()
    {
        if (YtDlpPath is null)
        {
            throw new InvalidOperationException("yt-dlp.exe was not found in the Tools folder next to the application.");
        }

        if (FfmpegPath is null)
        {
            throw new InvalidOperationException("ffmpeg.exe was not found in the Tools folder next to the application.");
        }

        if (FfprobePath is null)
        {
            throw new InvalidOperationException("ffprobe.exe was not found in the Tools folder next to the application.");
        }
    }

    public async Task<VideoInfo> FetchInfoAsync(string url, CancellationToken ct)
    {
        EnsureTools();

        var plan = _cookieAuthProvider();

        var psi = new ProcessStartInfo
        {
            FileName = YtDlpPath!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        psi.ArgumentList.Add("--dump-json");
        psi.ArgumentList.Add("--no-playlist");
        psi.ArgumentList.Add("--no-warnings");
        AddCookieArgs(psi.ArgumentList, plan);
        psi.ArgumentList.Add(url);

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start yt-dlp.exe.");

        using var cleanup = ct.Register(() => KillSafely(proc));

        var stdoutTask = proc.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = proc.StandardError.ReadToEndAsync(ct);

        try
        {
            await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
            await proc.WaitForExitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException(ct);
        }

        if (ct.IsCancellationRequested)
        {
            throw new OperationCanceledException(ct);
        }

        if (proc.ExitCode != 0)
        {
            var msg = ExtractError(await stderrTask.ConfigureAwait(false));
            _logger.Error($"yt-dlp could not fetch info for {url}: {msg}");
            throw new InvalidOperationException(FriendlyErrorMessage(msg, plan) ?? $"yt-dlp could not retrieve information for the video (exit code {proc.ExitCode}).");
        }

        var stdout = await stdoutTask.ConfigureAwait(false);
        foreach (var line in stdout.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 2 && trimmed[0] == '{')
            {
                try
                {
                    using var doc = JsonDocument.Parse(trimmed);
                    if (doc.RootElement.TryGetProperty("title", out var titleElement))
                    {
                        var title = titleElement.GetString();
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            return new VideoInfo(title);
                        }
                    }
                }
                catch (JsonException)
                {
                }
            }
        }

        return new VideoInfo(url);
    }

    public async Task<string> DownloadAsync(
        string url,
        string outputFilePath,
        int bitrate,
        CancellationToken ct,
        IProgress<DownloadProgressUpdate>? progress)
    {
        EnsureTools();

        var plan = _cookieAuthProvider();

        var ffmpegDir = Path.GetDirectoryName(FfmpegPath!) ?? string.Empty;
        var outputTemplate = outputFilePath.Replace("%", "%%");

        var psi = new ProcessStartInfo
        {
            FileName = YtDlpPath!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        psi.ArgumentList.Add("--newline");
        psi.ArgumentList.Add("--no-playlist");
        psi.ArgumentList.Add("--no-warnings");
        psi.ArgumentList.Add("--no-mtime");
        psi.ArgumentList.Add("-f");
        psi.ArgumentList.Add("bestaudio/best");
        psi.ArgumentList.Add("-x");
        psi.ArgumentList.Add("--audio-format");
        psi.ArgumentList.Add("mp3");
        psi.ArgumentList.Add("--audio-quality");
        psi.ArgumentList.Add($"{bitrate}K");
        psi.ArgumentList.Add("--ffmpeg-location");
        psi.ArgumentList.Add(ffmpegDir);
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add(outputTemplate);
        psi.ArgumentList.Add("--progress-template");
        psi.ArgumentList.Add("PROG|%(progress._percent_str)s|%(progress._speed_str)s|%(progress._eta_str)s|%(progress._total_bytes_str)s");
        AddCookieArgs(psi.ArgumentList, plan);
        psi.ArgumentList.Add(url);

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start yt-dlp.exe.");

        using var cleanup = ct.Register(() => KillSafely(proc));

        var errorBuilder = new StringBuilder();

        proc.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                HandleConsoleLine(e.Data, progress, errorBuilder);
            }
        };

        proc.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                HandleConsoleLine(e.Data, progress, errorBuilder);
            }
        };

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        try
        {
            await proc.WaitForExitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException(ct);
        }

        if (ct.IsCancellationRequested)
        {
            throw new OperationCanceledException(ct);
        }

        if (proc.ExitCode != 0)
        {
            var msg = ExtractError(errorBuilder.ToString());
            _logger.Error($"yt-dlp download failed ({url}): exit code {proc.ExitCode} :: {msg}");
            throw new InvalidOperationException(FriendlyErrorMessage(msg, plan) ?? $"yt-dlp failed with exit code {proc.ExitCode}.");
        }

        return outputFilePath;
    }

    private void HandleConsoleLine(string line, IProgress<DownloadProgressUpdate>? progress, StringBuilder errorBuilder)
    {
        if (line.StartsWith("PROG|", StringComparison.Ordinal))
        {
            var parts = line.Split('|');
            double? percent = null;
            if (parts.Length > 1)
            {
                var percentText = parts[1].Trim().TrimEnd('%');
                if (double.TryParse(percentText, NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
                {
                    percent = p;
                }
            }

            progress?.Report(new DownloadProgressUpdate(
                percent,
                parts.Length > 2 ? CleanMetric(parts[2]) : null,
                parts.Length > 3 ? CleanMetric(parts[3]) : null,
                parts.Length > 4 ? CleanMetric(parts[4]) : null,
                null));
            return;
        }

        if (line.IndexOf("[ExtractAudio]", StringComparison.Ordinal) >= 0
            || line.IndexOf("[VideoConvertor]", StringComparison.Ordinal) >= 0
            || line.StartsWith("[Merger]", StringComparison.Ordinal)
            || line.IndexOf("[Fixup", StringComparison.Ordinal) >= 0)
        {
            progress?.Report(new DownloadProgressUpdate(null, null, null, null, DownloadStatus.Converting));
            return;
        }

        if (line.Contains("ERROR:", StringComparison.OrdinalIgnoreCase))
        {
            lock (_errorLock)
            {
                errorBuilder.AppendLine(line);
            }
        }
    }

    private static string CleanMetric(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0 || trimmed.Equals("Unknown", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("N/A", StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }

        return trimmed;
    }

    private void AddCookieArgs(Collection<string> argumentList, CookieAuthPlan? plan)
    {
        if (plan is null)
        {
            return;
        }

        if (plan.Kind == CookieAuthKind.Browser)
        {
            var browser = plan.Value;
            if (browser is null || !AllowedCookieBrowsers.Contains(browser))
            {
                return;
            }

            argumentList.Add("--cookies-from-browser");
            argumentList.Add(browser.ToLowerInvariant());
            _logger.Info($"yt-dlp will use cookies from browser: {browser.ToLowerInvariant()}");
            return;
        }

        var path = plan.Value;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!File.Exists(path))
        {
            throw new InvalidOperationException("The selected cookies.txt file was not found. Re-select it in Settings and try again.");
        }

        argumentList.Add("--cookies");
        argumentList.Add(path);
        _logger.Info("yt-dlp will use the selected cookies.txt file");
    }

    private static string? FriendlyErrorMessage(string? message, CookieAuthPlan? plan)
    {
        if (message is null)
        {
            return null;
        }

        if (message.Contains("Sign in to confirm", StringComparison.OrdinalIgnoreCase))
        {
            return plan is not null
                ? "YouTube still requires sign-in even with cookies. Export a fresh cookies.txt while signed in (or try a different browser)."
                : "YouTube requires browser authentication for this video. Enable authentication in Settings and try again.";
        }

        if (message.Contains("Could not copy", StringComparison.OrdinalIgnoreCase))
        {
            return "Chrome cookies could not be accessed. Try closing Chrome completely, use Firefox, or select a cookies.txt file.";
        }

        if (message.Contains("Netscape format", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Invalid cookies file", StringComparison.OrdinalIgnoreCase))
        {
            return "The selected cookie file is not in Netscape format. Export cookies in Netscape (cookies.txt) format from your browser and try again.";
        }

        return message;
    }

    private static string? ExtractError(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return null;
        }

        var lines = stderr.Split('\n')
            .Where(l => l.Contains("ERROR:", StringComparison.OrdinalIgnoreCase)
                        || l.Contains("Unavailable", StringComparison.OrdinalIgnoreCase)
                        || l.Contains("returned an error", StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Trim())
            .Where(l => l.Length > 0);

        var text = string.Join(" ", lines).Trim();
        return text.Length == 0 ? null : (text.Length > 400 ? text[..400] : text);
    }

    private static void KillSafely(Process proc)
    {
        try
        {
            if (!proc.HasExited)
            {
                proc.Kill(entireProcessTree: true);
            }
        }
        catch
        {
        }
    }
}