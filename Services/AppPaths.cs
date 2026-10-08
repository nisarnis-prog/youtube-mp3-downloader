using System.IO;

namespace YouTube_MP3_Downloader.Services;

public static class AppPaths
{
    public static string AppDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Nisar",
        "YouTube MP3 Downloader");

    public static string LogsDirectory => Path.Combine(AppDataRoot, "Logs");

    public static string SettingsPath => Path.Combine(AppDataRoot, "settings.json");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(AppDataRoot);
        Directory.CreateDirectory(LogsDirectory);
    }
}