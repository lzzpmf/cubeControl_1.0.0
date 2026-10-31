; Compiled by Build-Installer.ps1 from the validated self-contained package.
#ifndef PackageRoot
  #error PackageRoot is required
#endif
#ifndef InstallerOutput
  #error InstallerOutput is required
#endif
#ifndef AppVersion
  #error AppVersion is required
#endif

[Setup]
AppId={{A7888648-CF5B-4D21-A220-70D98F55CF01}
AppName=cubeControl
AppVersion={#AppVersion}
AppPublisher=Luiz Paulo Martins Ferreira
VersionInfoVersion={#AppVersion}.0
DefaultDirName={autopf}\cubeControl
UsePreviousAppDir=no
DefaultGroupName=cubeControl
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
MinVersion=10.0.19041
OutputDir={#InstallerOutput}
OutputBaseFilename=CubeControl-{#AppVersion}-Setup-x64
SetupIconFile=..\..\CubeControl\Cube\windows\Cube.Desktop\Assets\Images\Icons\desktopIcon.ico
UninstallDisplayIcon={app}\CubeControl.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
Uninstallable=yes

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PackageRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\cubeControl"; Filename: "{app}\CubeControl.exe"; WorkingDir: "{app}"
Name: "{commondesktop}\cubeControl"; Filename: "{app}\CubeControl.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\CubeControl.exe"; Description: "{cm:LaunchProgram,cubeControl}"; Flags: nowait postinstall skipifsilent

; User preferences, recordings and backups outside {app} are never removed.
