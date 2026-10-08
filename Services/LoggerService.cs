using System.IO;

namespace YouTube_MP3_Downloader.Services;

public sealed class LoggerService : IDisposable
{
    private readonly object _lock = new();
    private readonly string _logDirectory;
    private readonly string _logFilePath;

    public LoggerService()
    {
        _logDirectory = AppPaths.LogsDirectory;
        AppPaths.EnsureCreated();
        _logFilePath = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
    }

    public string LogFilePath => _logFilePath;

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);
    public void Error(string message, Exception ex) => Write("ERROR", $"{message} :: {ex.GetType().Name}: {ex.Message}");

    private void Write(string level, string message)
    {
        var logDirectory = Path.GetDirectoryName(_logFilePath);
        if (logDirectory is null)
        {
            return;
        }

        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            lock (_lock)
            {
                Directory.CreateDirectory(logDirectory);
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
    }
}