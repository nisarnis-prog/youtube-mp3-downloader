namespace YouTube_MP3_Downloader.Models;

public sealed class Mp3Option
{
    public int Kbps { get; }

    public Mp3Option(int kbps) => Kbps = kbps;

    public string Label => $"{Kbps} kbps";
}