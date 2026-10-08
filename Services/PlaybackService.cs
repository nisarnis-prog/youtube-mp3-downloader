using System.IO;
using System.Windows.Media;

namespace YouTube_MP3_Downloader.Services;

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}

public sealed class PlaybackService : IDisposable
{
    private readonly LoggerService _logger;
    private MediaPlayer? _player;
    private string? _currentFilePath;
    private PlaybackState _state = PlaybackState.Stopped;
    private TimeSpan _duration;
    private bool _hasDuration;
    private TimeSpan _pendingStartPosition;
    private string? _pendingStartPath;
    private bool _disposed;

    public PlaybackService(LoggerService logger)
    {
        _logger = logger;
    }

    public event EventHandler? StateChanged;

    public event EventHandler<string>? Failure;

    public PlaybackState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string? CurrentFilePath => _currentFilePath;

    public TimeSpan CurrentPosition => _player is not null ? _player.Position : TimeSpan.Zero;

    public TimeSpan Duration => _duration;

    public bool HasDuration => _hasDuration;

    public void Play(string filePath)
    {
        if (_disposed)
        {
            return;
        }

        if (!File.Exists(filePath))
        {
            HandleFailure($"The audio file is no longer available: {Path.GetFileName(filePath)}");
            return;
        }

        if (string.Equals(_currentFilePath, filePath, StringComparison.OrdinalIgnoreCase) && _player is not null)
        {
            _player.Play();
            _logger.Info($"Playback resumed: {Path.GetFileName(filePath)}");
            State = PlaybackState.Playing;
            return;
        }

        StopPlayer();

        _currentFilePath = filePath;
        _duration = TimeSpan.Zero;
        _hasDuration = false;
        _player = new MediaPlayer();
        _player.MediaOpened += OnMediaOpened;
        _player.MediaEnded += OnMediaEnded;
        _player.MediaFailed += OnMediaFailed;
        _player.Open(new Uri(filePath, UriKind.Absolute));
        _player.Play();
        _logger.Info($"Playback started: {Path.GetFileName(filePath)}");
        State = PlaybackState.Playing;
    }

    public void Pause()
    {
        if (_disposed || _player is null || _state != PlaybackState.Playing)
        {
            return;
        }

        _player.Pause();
        _logger.Info("Playback paused");
        State = PlaybackState.Paused;
    }

    public void Resume()
    {
        if (_disposed || _player is null || _state != PlaybackState.Paused)
        {
            return;
        }

        _player.Play();
        _logger.Info("Playback resumed");
        State = PlaybackState.Playing;
    }

    public void Stop()
    {
        if (_disposed)
        {
            return;
        }

        StopPlayer();
        _logger.Info("Playback stopped");
        State = PlaybackState.Stopped;
    }

    public void Seek(TimeSpan position)
    {
        if (_disposed)
        {
            return;
        }

        if (position < TimeSpan.Zero)
        {
            position = TimeSpan.Zero;
        }

        if (_hasDuration && position > _duration)
        {
            position = _duration;
        }

        if (_player is null)
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                _pendingStartPosition = position;
                _pendingStartPath = _currentFilePath;
            }

            return;
        }

        var before = _player.Position.TotalSeconds;
        try
        {
            var wasPlaying = _state == PlaybackState.Playing;
            if (wasPlaying)
            {
                _player.Pause();
            }

            _player.Position = position;

            if (wasPlaying)
            {
                _player.Play();
            }
        }
        catch (Exception)
        {
            _logger.Warn($"Seek to {position.TotalSeconds:F1}s failed");
            return;
        }

        var after = _player.Position.TotalSeconds;
        _logger.Info($"Player position before={before:F1}s");
        _logger.Info($"Player position after={after:F1}s");
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        if (_player is null)
        {
            return;
        }

        if (_player.NaturalDuration.HasTimeSpan)
        {
            _duration = _player.NaturalDuration.TimeSpan;
            _hasDuration = true;
        }
        else
        {
            _duration = TimeSpan.Zero;
            _hasDuration = false;
        }

        if (_pendingStartPosition > TimeSpan.Zero
            && string.Equals(_pendingStartPath, _currentFilePath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _player.Position = _pendingStartPosition;
                _logger.Info($"Playback opened, starting at pending {_pendingStartPosition.TotalSeconds:F1}s");
            }
            catch (Exception)
            {
                _logger.Warn("Pending start position could not be applied");
            }
        }

        _pendingStartPosition = TimeSpan.Zero;
        _pendingStartPath = null;
    }

    private void OnMediaEnded(object? sender, EventArgs e)
    {
        _logger.Info("Playback completed");
        StopPlayer();
        State = PlaybackState.Stopped;
    }

    private void OnMediaFailed(object? sender, ExceptionEventArgs e)
    {
        var reason = e.ErrorException is not null
            ? $"{e.ErrorException.Message} ({(int)(e.ErrorException.HResult & 0xFFFF)})"
            : "Unknown media error";
        HandleFailure(reason);
    }

    private void HandleFailure(string reason)
    {
        _logger.Warn($"Playback failed: {reason}");
        StopPlayer();
        State = PlaybackState.Stopped;
        Failure?.Invoke(this, reason);
    }

    private void StopPlayer()
    {
        if (_player is not null)
        {
            _player.MediaOpened -= OnMediaOpened;
            _player.MediaEnded -= OnMediaEnded;
            _player.MediaFailed -= OnMediaFailed;
            _player.Stop();
            _player.Close();
            _player = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopPlayer();
        State = PlaybackState.Stopped;
        _currentFilePath = null;
        _pendingStartPosition = TimeSpan.Zero;
        _pendingStartPath = null;
    }
}