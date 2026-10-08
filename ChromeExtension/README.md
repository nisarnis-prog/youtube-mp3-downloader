# Nisar MP3 Downloader Companion — Local Development Testing

Send the currently open YouTube URL from Chrome to the local **YouTube MP3 Downloader** desktop app.

## Architecture

```
Chrome extension (popup.js)
    |  chrome.runtime.sendNativeMessage("com.nisar.youtube_mp3_downloader", {action, url})
    v
YouTubeMp3NativeHost.exe        (NativeHost\ — reads stdin/stdout Native Messaging frames)
    |  named pipe: \\.\pipe\NisarYouTubeMp3Downloader
    v
YouTube MP3 Downloader.exe      (named-pipe listener started by the WPF app)
```

- No HTTP server, no localhost web API, no file writes from Chrome.
- Chrome talks only to the native host; the native host talks only to the running desktop app.
- All URL validation is repeated inside the desktop app (defense in depth). The native host accepts only the actions `add` and `addAndStart` and rejects everything else.

## Prerequisites

- Chrome (with Native Messaging available; if it is disabled, enable the flag `chrome://flags/#enable-native-messaging` and restart Chrome).
- .NET 10 SDK for building both projects.

## Setup Steps

1. **Build the desktop app**

   ```powershell
   dotnet build -c Debug "PROJECT_ROOT\YouTube-MP3-Downloader.csproj"
   ```

2. **Build the native host**

   ```powershell
   dotnet build -c Debug "PROJECT_ROOT\NativeHost\YouTubeMp3NativeHost.csproj"
   ```

3. **Launch the desktop app**

   Run `bin\Debug\net10.0-windows\YouTube MP3 Downloader.exe`.
   (It starts the named-pipe listener `NisarYouTubeMp3Downloader` automatically — you should see
   `Named pipe IPC server started` in `Logs\app_yyyyMMdd.log`.)

4. **Open `chrome://extensions`**

5. **Enable Developer mode** (toggle in the top-right corner).

6. **Load unpacked** and select the folder:

   ```
   PROJECT_ROOT\ChromeExtension\
   ```

7. **Copy the generated Chrome extension ID**

   On the extension card, click **Details** and copy the ID (a 32-character hex string).

8. **Update the native-host manifest `allowed_origins`**

   Run the registration script **with the extension ID** — it writes a generated manifest
   (next to the host exe) whose `allowed_origins` contains your ID:

   ```powershell
   .\Scripts\Register-ChromeNativeHost.ps1 -ExtensionId <THE_COPIED_ID>
   ```

   (If you skip `-ExtensionId`, the manifest keeps the `chrome-extension://<EXTENSION_ID>/`
   placeholder and Chrome will reject the host. Re-run the script with the ID, or edit
   `NativeHost\bin\Debug\net10.0\com.nisar.youtube_mp3_downloader.json` directly.)

9. **Restart Chrome** if required (usually only if `chrome://extensions` was open before registering).

10. **Open a normal YouTube video**, e.g. `https://www.youtube.com/watch?v=...` or `https://youtu.be/...`.

11. **Click the extension icon** («Nisar MP3 Downloader Companion»).

12. **Test Add to Queue** — the URL appears in the desktop app queue as `Waiting`
    (no download starts).

13. **Test Add & Start** — the URL is added and downloads begin according to the
    app's current 1–5 concurrency setting.

## Unsupported pages

Playlists, channels, shorts-redirects without a `watch?v=`/`youtu.be` id, and any non-YouTube page
show **This page is not a supported YouTube video.** and nothing is sent.

## Manual step required for the Chrome extension ID

Chrome generates a **random extension ID** when you load the folder unpacked, and it can change if
you remove/re-add the extension. The native host manifest's `allowed_origins` must contain the
exact current ID. Whenever the ID changes: copy the new ID from `chrome://extensions` and
re-run `Register-ChromeNativeHost.ps1 -ExtensionId <NEW_ID>`.

## Native Messaging message format

Extension → host → desktop app (framed with a 4-byte little-endian length, UTF-8 JSON):

```json
{ "action": "add",         "url": "https://..." }
{ "action": "addAndStart", "url": "https://..." }
```

Desktop app → host → extension:

```json
{ "success": true,  "message": "Link sent to downloader.",              "added": 1, "skipped": 0 }
{ "success": false, "message": "Unsupported YouTube URL." }
{ "success": false, "message": "Downloader is not running." }
{ "success": false, "message": "Unknown action." }
```

## Troubleshooting

- **"Could not connect: Specified native messaging host not found"** → the host is not
  registered (re-run step 8), or Chrome wasn't restarted (step 9).
- **"Native host has exited" / "Downloader is not running"** → the desktop app is not
  running (or is shutting down).
- **"Access to the specified native messaging host is forbidden"** → `allowed_origins` does not
  match your current extension ID (repeat step 8 with the correct ID).
- The desktop app never mounts files from Chrome; the queue is filled only through the
  named pipe, so the folder paths browsers can reference are never sent back.