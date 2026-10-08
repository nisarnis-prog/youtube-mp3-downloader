using System.Windows;
using YouTube_MP3_Downloader.Services;
using YouTube_MP3_Downloader.ViewModels;

namespace YouTube_MP3_Downloader.Views;

public partial class MainWindow : Window
{
    private readonly LoggerService _logger;

    public MainWindow(MainViewModel viewModel, YtDlpService ytDlp, LoggerService logger)
    {
        InitializeComponent();
        DataContext = viewModel;
        _logger = logger;

        var missing = ytDlp.GetMissingTools();
        if (missing.Count > 0)
        {
            var message = "The following required tools were not found in the Tools folder next to the application:\n\n"
                          + string.Join("\n", missing.Select(t => "  • " + t))
                          + "\n\nPlace them in the Tools folder and restart the application.";
            _logger.Error("Missing tools at startup: " + string.Join(", ", missing));
            MessageBox.Show(this, message, "Missing Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else
        {
            _logger.Info("All required tools found (yt-dlp, ffmpeg, ffprobe).");
        }
    }
}