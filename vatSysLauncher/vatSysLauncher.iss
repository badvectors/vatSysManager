[Setup]
AppName=vatSys Launcher
AppVersion=1.22
AppVerName=vatSys Launcher
DefaultDirName={autopf}\vatSys Launcher
OutputBaseFilename=vatSys Launcher
SourceDir=C:\Users\ajdun\source\repos\vatSysManager\vatSysLauncher\bin\Release\net9.0-windows\
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\icon.ico

[Files]
Source: "*"; DestDir: "{app}"

[Icons]
Name: "{commondesktop}\vatSys Launcher"; Filename: "{app}\vatSysLauncher.exe"; IconFilename: "{app}\icon.ico"
Name: "{commonprograms}\vatSys Launcher"; Filename: "{app}\vatSysLauncher.exe"; IconFilename: "{app}\icon.ico"

[Run]
Filename: "{app}\vatSysLauncher.exe"; \
    Flags: postinstall