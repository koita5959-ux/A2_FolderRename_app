@echo off
chcp 65001 >nul
echo === DesktopKit.FolderRename ビルド＆パブリッシュ ===
cd %~dp0FolderRename
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ..\publish\
if errorlevel 1 goto :error

echo.
echo === インストーラー作成 ===
cd %~dp0
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss
if errorlevel 1 goto :error

echo.
echo === ZIP パック作成 ===
cd %~dp0
powershell -NoProfile -Command "Compress-Archive -Path 'installer\FolderRename_Setup_v1.01.exe','ご利用ガイド.txt' -DestinationPath 'FolderRename1.01_installer.zip' -Force"
if errorlevel 1 goto :error

echo.
echo 完了しました。FolderRename1.01_installer.zip を作成しました。
pause
exit /b 0

:error
echo.
echo エラーが発生しました。
pause
exit /b 1
