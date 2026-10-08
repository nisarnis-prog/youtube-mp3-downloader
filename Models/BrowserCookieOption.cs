namespace YouTube_MP3_Downloader.Models;

public sealed class BrowserCookieOption
{
    public string Name { get; }
    public string Value { get; }

    public BrowserCookieOption(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public sealed class CookieMethodOption
{
    public string Name { get; }
    public bool UsesFile { get; }

    public CookieMethodOption(string name, bool usesFile)
    {
        Name = name;
        UsesFile = usesFile;
    }
}

public enum CookieAuthKind
{
    Browser,
    File
}

public sealed record CookieAuthPlan(CookieAuthKind Kind, string Value);