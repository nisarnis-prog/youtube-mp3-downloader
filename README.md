
# YouTube MP3 Downloader

A Windows desktop application for downloading YouTube audio as MP3, with multiple-link queue management, concurrent downloads, a built-in MP3 player, and optional Chrome integration.

**Multiple links. One queue. Up to five concurrent downloads.**

## Application Screenshots

### Home — Multiple-Link Batch Downloads

Paste multiple YouTube links, configure MP3 quality and concurrent downloads, and manage everything from one queue.

![YouTube MP3 Downloader Home](Screenshots/Home.png)

### Downloads — Queue and Built-in MP3 Player

Track completed downloads, manage individual items, and play downloaded MP3 files directly in the application.

![YouTube MP3 Downloader Downloads](Screenshots/Downloads.png)

### Dark Theme — Modern Dark Interface

Switch between light and dark themes while managing multiple downloads, MP3 quality, and your download queue.

![YouTube MP3 Downloader Dark Theme](Screenshots/DarkTheme.png)

## Features

- **Multiple-link downloading:** Paste several YouTube video URLs at once, one per line.
- **Concurrent downloads:** Configure between 1 and 5 simultaneous downloads.
- **MP3 quality selection:** Choose 128, 192, 256, or 320 kbps.
- **Download queue management:** Start downloads, pause or resume queue scheduling, cancel downloads, and clear completed items.
- **Built-in MP3 player:** Play downloaded MP3s or select existing files and folders.
- **Chrome companion extension:** Send the currently open supported YouTube video directly to the desktop application.
- **Light and dark themes:** Choose your preferred appearance.
- **Local Windows application:** No separate cloud account is required.

## Download and Install

Download `YouTube-MP3-Downloader-Setup-v1.0.0.exe` from the GitHub Releases page once the first release is published.

1. Run the installer.
2. Follow the installation wizard.
3. Optionally select **Create a desktop shortcut**.
4. Select **Open Chrome Extension Setup Instructions** if you want Chrome integration.
5. Complete installation and launch YouTube MP3 Downloader.

The installer is currently unsigned. Windows may display a SmartScreen warning. Verify that the installer comes from the official repository before deciding whether to run it.

The Chrome extension is optional. All direct downloading features are available through the desktop application.

## Download Multiple YouTube Links

1. Open YouTube MP3 Downloader.
2. Copy one or more supported YouTube video URLs.
3. Paste them into the link input area, placing each URL on a separate line.
4. Add the links to the download queue.
5. Choose your preferred MP3 quality and output folder.
6. Configure the number of concurrent downloads (1–5).
7. Start downloading.

You can monitor individual items and manage the queue without adding each link separately.

## Optional Chrome Companion Extension

The Chrome extension lets you send the currently open supported YouTube video to the desktop application.

After installing the desktop app:

1. Open Chrome and navigate to `chrome://extensions`.
2. Enable **Developer mode**.
3. Click **Load unpacked**.
4. Select the `ChromeExtension` folder inside the application installation directory.
5. Confirm that **Nisar MP3 Downloader Companion** appears.

Keep the desktop application running when using the extension.

The extension provides two actions:

- **Add to Queue:** Sends the current video to the queue without starting downloads.
- **Add & Start:** Sends the video and starts queue processing.

Expected extension ID: `degdafbfgoodjcnhbnialnbdfahlfoce`

## System Requirements

**For installed users:**

- Windows 10 (version 1809 or later) or Windows 11, 64-bit
- Google Chrome only if using the optional companion extension

**For developers:**

- .NET 10 SDK
- Windows development environment with WPF support
- Internet connection for the initial automated dependency setup
- Inno Setup 6 (only required for installer packaging)

The installer is designed as a self-contained Windows build, so end users do not need to install the .NET SDK.

## Build from Source

Clone the repository:

```powershell
git clone https://github.com/nisarnis-prog/youtube-mp3-downloader.git
cd youtube-mp3-downloader
```

### Step 1 — Install Required Dependencies

Run the automated dependency setup script from the project directory:

```powershell
.\Scripts\Setup-Dependencies.ps1
```

The script automatically downloads tested versions of the required tools:

- **yt-dlp:** v2026.08.19
- **FFmpeg:** Pinned Windows x64 LGPL build
- **ffprobe:** Included with the pinned FFmpeg build

The dependencies are downloaded from their respective GitHub release distributions.

Each executable is verified against its pinned SHA256 checksum before installation into the project's `Tools` directory.

If all three executables already match the expected hashes, the script skips downloading them.

The script refuses to overwrite existing binaries with different hashes unless explicitly authorized using:

```powershell
.\Scripts\Setup-Dependencies.ps1 -ReplaceExisting
```

Windows PowerShell may display a security warning for downloaded scripts. Review the script before choosing to run it.

### Step 2 — Build the Desktop Application

```powershell
dotnet build .\YouTube-MP3-Downloader.csproj -c Release
```

### Step 3 — Build the Optional Chrome Native Messaging Host

```powershell
dotnet build .\NativeHost\YouTubeMp3NativeHost.csproj -c Release
```

The Chrome native messaging host is required only for optional Chrome extension integration.

The desktop application supports direct YouTube link downloads, including multiple-link batch downloads, without the Chrome extension.

### Installer Packaging

Creating a Windows installer additionally requires the appropriate self-contained publish output and Inno Setup 6.

The automated dependency setup is intended for developers building from source. Redistribution of third-party binaries must comply with their respective licensing requirements.

## Troubleshooting

**The Chrome extension cannot connect:** Make sure the desktop application is running and that the native messaging host is correctly registered.

**The Chrome extension reports forbidden access:** Verify that its ID matches the ID in the native host's `allowed_origins`.

**A video fails to download:** Check your connection and whether the content is available. YouTube authentication or anti-bot checks can affect individual downloads.

**The dependency setup script is blocked:** Review the script and its source before allowing PowerShell to run it.

**The dependency setup reports a SHA256 mismatch:** The downloaded or existing executable does not match the pinned tested version. Do not bypass checksum verification. Check the download source and version before proceeding.

**The installer shows an unknown publisher warning:** The current installer is not digitally code-signed. Verify the download source and file integrity before proceeding.

## Responsible Use

Use this application only for content you own, have permission to download, or are otherwise legally authorized to download. Users are responsible for complying with applicable copyright laws and platform terms.

This is an independent project and is not affiliated with or endorsed by YouTube or Google.

## Third-Party Components

This application uses yt-dlp and FFmpeg for downloading and media processing.

Their respective licenses, notices, and applicable source distribution requirements must be respected when building or redistributing the application.

The Windows installer release is pending completion of the third-party licensing review.

## Author

**Nisar**

Version 1.0.0 — Initial public source release.
