using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YouTube_MP3_Downloader.Models;

public enum DownloadStatus
{
    Waiting,
    Preparing,
    Downloading,
    Converting,
    Completed,
    Failed,
    Cancelled
}

public sealed class DownloadItem : INotifyPropertyChanged
{
    private string _title = "";
    private DownloadStatus _status = DownloadStatus.Waiting;
    private double _progress;
    private string _speed = "";
    private string _fileSize = "";
    private string _errorMessage = "";
    private string _eta = "";
    private double _estimatedTotalBytes;
    private double _downloadedBytes;
    private bool _isCurrentTrack;
    private bool _isCurrentTrackPlaying;

    public string Id { get; } = Guid.NewGuid().ToString("N");
    public string Url { get; init; } = "";
    public string OutputFilePath { get; set; } = "";

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public DownloadStatus Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(CanCancel)); OnPropertyChanged(nameof(CanRetry)); OnPropertyChanged(nameof(CanRemove)); OnPropertyChanged(nameof(CanPlay)); }
    }

    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    public string Speed
    {
        get => _speed;
        set { _speed = value; OnPropertyChanged(); }
    }

    public string FileSize
    {
        get => _fileSize;
        set { _fileSize = value; OnPropertyChanged(); }
    }

    public string Eta
    {
        get => _eta;
        set { _eta = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public double EstimatedTotalBytes
    {
        get => _estimatedTotalBytes;
        set { _estimatedTotalBytes = value; OnPropertyChanged(); }
    }

    public double DownloadedBytes
    {
        get => _downloadedBytes;
        set { _downloadedBytes = value; OnPropertyChanged(); }
    }

    public bool IsCurrentTrack
    {
        get => _isCurrentTrack;
        set
        {
            if (_isCurrentTrack != value)
            {
                _isCurrentTrack = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsCurrentTrackPlaying
    {
        get => _isCurrentTrackPlaying;
        set
        {
            if (_isCurrentTrackPlaying != value)
            {
                _isCurrentTrackPlaying = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusText
    {
        get
        {
            return Status switch
            {
                DownloadStatus.Waiting => "Waiting",
                DownloadStatus.Preparing => "Preparing",
                DownloadStatus.Downloading => "Downloading",
                DownloadStatus.Converting => "Converting",
                DownloadStatus.Completed => "Completed",
                DownloadStatus.Failed => "Failed",
                DownloadStatus.Cancelled => "Cancelled",
                _ => Status.ToString()
            };
        }
    }

    public bool CanCancel => Status == DownloadStatus.Preparing || Status == DownloadStatus.Downloading || Status == DownloadStatus.Converting;
    public bool CanRetry => Status == DownloadStatus.Failed || Status == DownloadStatus.Cancelled;
    public bool CanRemove => Status == DownloadStatus.Waiting || Status == DownloadStatus.Completed || Status == DownloadStatus.Failed || Status == DownloadStatus.Cancelled;
    public bool CanPlay => Status == DownloadStatus.Completed;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}