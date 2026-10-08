; YouTube MP3 Downloader - Inno Setup 6 installer script
; Product: YouTube MP3 Downloader 1.0.0 (publisher: Nisar)
; Payload source: publish\final-win-x64 (frozen Release, self-contained, win-x64)
; Per-user install, no admin required, 64-bit only.

#define MyAppName "YouTube MP3 Downloader"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Nisar"
#define MyAppExeName "YouTube MP3 Downloader.exe"
#define MyAppId "{A505CB8D-AA63-43E2-A855-35CF4178F46B}"

[Setup]
AppId={{#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppComments=YouTube to MP3 downloader with a Chrome companion extension
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
DefaultDirName={localappdata}\Programs\Nisar\{#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename=YouTube-MP3-Downloader-Setup-v{#MyAppVersion}
SetupIconFile=..\Assets\AppIcon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0.17763
ShowLanguageDialog=no
SetupLogging=yes
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName} Installer
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "launchapp"; Description: "Run {#MyAppName} after installation"; GroupDescription: "Optional actions:"
Name: "openinstr"; Description: "Open Chrome Extension Setup Instructions"; GroupDescription: "Optional actions:"; Flags: unchecked
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\final-win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Google\Chrome\NativeMessagingHosts\com.nisar.youtube_mp3_downloader"; ValueType: string; ValueName: ""; ValueData: "{app}\NativeHost\com.nisar.youtube_mp3_downloader.json"; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent; Tasks: launchapp
Filename: "{sys}\notepad.exe"; Parameters: """{app}\Chrome Extension Setup.txt"""; Description: "Open Chrome Extension Setup Instructions"; Flags: nowait postinstall skipifsilent; Tasks: openinstr

[Code]
function EscapeJsonPath(const PathName: string): string;
var
  I: Integer;
  ResultStr: string;
begin
  ResultStr := '';
  for I := 1 to Length(PathName) do
  begin
    if PathName[I] = '\' then
      ResultStr := ResultStr + '\\'
    else if PathName[I] = '"' then
      ResultStr := ResultStr + '\"'
    else
      ResultStr := ResultStr + PathName[I];
  end;
  Result := ResultStr;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  JsonPath: string;
  InstalledHostExe: string;
  Json: string;
  TxtPath: string;
  Txt: string;
begin
  if CurStep = ssPostInstall then
  begin
    JsonPath := ExpandConstant('{app}\NativeHost\com.nisar.youtube_mp3_downloader.json');
    InstalledHostExe := ExpandConstant('{app}\NativeHost\YouTubeMp3NativeHost.exe');

    Json := '{' + #13#10 +
      '  "name": "com.nisar.youtube_mp3_downloader",' + #13#10 +
      '  "description": "Native messaging host for Nisar MP3 Downloader Companion",' + #13#10 +
      '  "path": "' + EscapeJsonPath(InstalledHostExe) + '",' + #13#10 +
      '  "type": "stdio",' + #13#10 +
      '  "allowed_origins": [' + #13#10 +
      '    "chrome-extension://degdafbfgoodjcnhbnialnbdfahlfoce/"' + #13#10 +
      '  ]' + #13#10 +
      '}';

    if SaveStringToFile(JsonPath, Json, False) then
      Log('NativeHost manifest written to: ' + JsonPath)
    else
      Log('ERROR: failed to write NativeHost manifest.');

    TxtPath := ExpandConstant('{app}\Chrome Extension Setup.txt');
    Txt := 'Chrome Companion Extension Setup' + #13#10 + #13#10 +
      '1. Open Google Chrome.' + #13#10 +
      '2. Go to chrome://extensions' + #13#10 +
      '3. Turn on Developer mode.' + #13#10 +
      '4. Click Load unpacked.' + #13#10 +
      '5. Select this folder:' + #13#10 +
      ExpandConstant('{app}\ChromeExtension') + #13#10 + #13#10 +
      'Expected extension ID:' + #13#10 +
      'degdafbfgoodjcnhbnialnbdfahlfoce' + #13#10 + #13#10 +
      'After loading the extension, restart Chrome once if Native Messaging does not connect immediately.' + #13#10;

    if SaveStringToFile(TxtPath, Txt, False) then
      Log('Chrome Extension Setup instructions written to: ' + TxtPath)
    else
      Log('ERROR: failed to write Chrome Extension Setup instructions.');
  end;
end;
