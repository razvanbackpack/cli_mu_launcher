@echo off
echo Building GG Mu Launcher...

REM Restore dependencies
dotnet restore

REM Build in Release mode
dotnet build -c Release

REM Publish as single file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "./dist"

REM Copy assets
if not exist "dist\assets" mkdir "dist\assets"
copy "assets\icon.png" "dist\assets\"
copy "content.md" "dist\"

REM Rename executable
ren "dist\GGMuLauncher.exe" "Mu Launcherexe"

echo.
echo Build complete! Executable is in the 'dist' folder.
pause