using System.IO;

namespace YouTube_MP3_Downloader.Models;

public sealed class LocalTrack
{
    public LocalTrack(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public string FileName => Path.GetFileName(FilePath);

    public string Title => Path.GetFileNameWithoutExtension(FilePath);

    public override string ToString() => FileName;
}