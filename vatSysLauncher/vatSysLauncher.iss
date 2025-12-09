[Setup]
AppName=vatSys Launcher
AppVersion=1.0
DefaultDirName={autopf}\vatSys Launcher

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Files]
Source: "C:\Users\ajdun\source\repos\vatSysManager\vatSysLauncher\bin\Release\net9.0-windows\*"; DestDir: "{app}"