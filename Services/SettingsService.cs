using System.IO;
using System.Text.Json;

namespace YouTube_MP3_Downloader.Services;

public sealed class AppSettings
{
    public string OutputFolder { get; set; } = "";
    public int MaxConcurrentDownloads { get; set; } = 3;
    public int Mp3Bitrate { get; set; } = 320;
    public string Theme { get; set; } = "Dark";
    public bool UseBrowserCookies { get; set; }
    public string BrowserCookieSource { get; set; } = "chrome";
    public bool UseCookieFile { get; set; }
    public string CookieFilePath { get; set; } = "";
}

public sealed class SettingsService
{
    private const string SettingsFileName = "settings.json";
    private readonly string _settingsPath;
    private readonly LoggerService _logger;

    public SettingsService(LoggerService logger)
    {
        _logger = logger;
        _settingsPath = AppPaths.SettingsPath;
    }

    public AppSettings Load()
    {
        var settings = new AppSettings
        {
            OutputFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "YouTube MP3")
        };

        try
        {
            if (File.Exists(_settingsPath))
            {
                var loaded = ReadSettings(_settingsPath);
                if (loaded is not null)
                {
                    settings = loaded;
                }
            }
            else
            {
                MigrateFromLegacyLocation();
                var migrated = ReadSettings(_settingsPath);
                if (migrated is not null)
                {
                    settings = migrated;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load settings", ex);
        }

        if (string.IsNullOrWhiteSpace(settings.OutputFolder))
        {
            settings.OutputFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "YouTube MP3");
        }

        if (string.IsNullOrWhiteSpace(settings.BrowserCookieSource)
            || !IsAllowedBrowserSource(settings.BrowserCookieSource))
        {
            settings.BrowserCookieSource = "chrome";
        }
        else
        {
            settings.BrowserCookieSource = settings.BrowserCookieSource.ToLowerInvariant();
        }

        if (settings.MaxConcurrentDownloads is < 1 or > 5)
        {
            settings.MaxConcurrentDownloads = 3;
        }

        if (settings.Mp3Bitrate is not (128 or 192 or 256 or 320))
        {
            settings.Mp3Bitrate = 320;
        }

        if (!string.Equals(settings.Theme, "Light", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(settings.Theme, "Dark", StringComparison.OrdinalIgnoreCase))
        {
            settings.Theme = "Dark";
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var dir = Path.GetDirectoryName(_settingsPath);
            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            _logger.Info($"Settings saved: output folder='{settings.OutputFolder}', max concurrent={settings.MaxConcurrentDownloads}, bitrate={settings.Mp3Bitrate}, theme={settings.Theme}, use browser cookies={settings.UseBrowserCookies}, browser cookie source={settings.BrowserCookieSource}, use cookie file={settings.UseCookieFile}, cookie file defined={!string.IsNullOrWhiteSpace(settings.CookieFilePath)}");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to save settings", ex);
        }
    }

    private AppSettings? ReadSettings(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to read settings file", ex);
            return null;
        }
    }

    private void MigrateFromLegacyLocation()
    {
        try
        {
            var legacyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
            if (!File.Exists(_settingsPath) && File.Exists(legacyPath))
            {
                var dir = Path.GetDirectoryName(_settingsPath);
                if (dir is not null)
                {
                    Directory.CreateDirectory(dir);
                }

                File.Copy(legacyPath, _settingsPath);
                _logger.Info("Settings were migrated from the previous executable directory.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Settings migration failed", ex);
        }
    }

    private static bool IsAllowedBrowserSource(string value)
    {
        return value.Equals("chrome", StringComparison.OrdinalIgnoreCase)
               || value.Equals("edge", StringComparison.OrdinalIgnoreCase)
               || value.Equals("firefox", StringComparison.OrdinalIgnoreCase);
    }
}