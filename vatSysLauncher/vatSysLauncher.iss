[Setup]
AppName=vatSys Launcher
AppVersion=1.4
AppVerName=vatSys Launcher
DefaultDirName={autopf}\vatSys Launcher
OutputBaseFilename=vatSys Launcher
SourceDir=c:\Users\ajdun\source\repos\vatSysManager\vatSysLauncher\bin\Release\net9.0-windows\
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\icon.ico

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Files]
Source: "*"; DestDir: "{app}"

[Icons]
Name: "{commondesktop}\vatSys Launcher"; Filename: "{autopf}\vatSys Launcher.exe"; IconFilename: "{app}\icon.ico"
Name: "{commonprograms}\vatSys Launcher"; Filename: "{autopf}\vatSys Launcher.exe"; IconFilename: "{app}\icon.ico"

[Run]
; Runs the app after interactive install, but skips if silent
Filename: "{app}\vatSysLauncher.exe"; \
    Flags: nowait postinstall skipifsilent