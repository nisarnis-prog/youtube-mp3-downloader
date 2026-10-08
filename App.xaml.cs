using System.Windows;
using YouTube_MP3_Downloader.Services;
using YouTube_MP3_Downloader.ViewModels;
using YouTube_MP3_Downloader.Views;

namespace YouTube_MP3_Downloader;

public partial class App : Application
{
    private LoggerService? _logger;
    private YtDlpService? _ytDlp;
    private DownloadQueueService? _queue;
    private MainViewModel? _mainViewModel;
    private SettingsService? _settingsService;
    private IpcServerService? _ipcServer;

    public MainViewModel? MainViewModel => _mainViewModel;
    public DownloadQueueService? QueueService => _queue;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _logger = new LoggerService();
        _logger.Info("Application starting");

        _settingsService = new SettingsService(_logger);
        _ytDlp = new YtDlpService(_logger, () => _mainViewModel?.CookieAuthPlan);
        _queue = new DownloadQueueService(_ytDlp, _logger, () => _mainViewModel?.OutputFolder ?? "", () => _mainViewModel?.SelectedMp3Option?.Kbps ?? 320);

        _mainViewModel = new MainViewModel(_settingsService, _queue, _logger);
        _mainViewModel.ThemeSwitchRequested += (_, isDark) => ApplyTheme(isDark);
        ApplyTheme(_mainViewModel.IsDarkTheme);

        _ipcServer = new IpcServerService(_logger, (url, autoStart) =>
        {
            var vm = _mainViewModel;
            if (vm is null)
            {
                return Task.FromResult(new IpcSubmitResult(false, "Downloader is not available.", 0, 0));
            }

            return vm.SubmitExternalUrlAsync(url, autoStart);
        });
        _logger.Info($"Named pipe IPC server started ({IpcServerService.PipeName}).");

        var window = new MainWindow(_mainViewModel, _ytDlp, _logger);
        MainWindow = window;
        window.Show();
    }

    private void ApplyTheme(bool isDark)
    {
        try
        {
            var uri = new Uri(isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative);
            var dictionary = new ResourceDictionary { Source = uri };
            if (Resources.MergedDictionaries.Count == 0)
            {
                Resources.MergedDictionaries.Add(dictionary);
            }
            else
            {
                Resources.MergedDictionaries[0] = dictionary;
            }

            _logger?.Info($"Theme applied: {(isDark ? "Dark" : "Light")}");
        }
        catch (Exception ex)
        {
            _logger?.Error("Failed to apply theme", ex);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.Info("Application exiting");
        _ipcServer?.Dispose();
        _queue?.Dispose();
        _mainViewModel?.ShutdownPlayback();
        base.OnExit(e);
    }
}