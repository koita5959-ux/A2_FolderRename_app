[Setup]
AppName=FolderRename
AppVersion=1.00
AppPublisher=Media House
AppPublisherURL=https://app.media-house.jp/
AppSupportURL=https://app.media-house.jp/
DefaultDirName={autopf}\FolderRename
DefaultGroupName=FolderRename
OutputBaseFilename=FolderRename_Setup_v1.00
OutputDir=installer
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName=FolderRename

[Files]
Source: "publish\DesktopKit.FolderRename.exe"; DestDir: "{app}"; DestName: "FolderRename.exe"; Flags: ignoreversion
Source: "publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "ご利用ガイド.txt"; DestDir: "{app}"; Flags: ignoreversion
; pdbは含めない

[Icons]
Name: "{group}\FolderRename"; Filename: "{app}\FolderRename.exe"
Name: "{group}\アンインストール"; Filename: "{uninstallexe}"
