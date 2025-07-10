#!/bin/bash
echo "Building Mu Launcher for Windows on Linux..."

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found."
    exit 1
fi

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build in Release mode
echo "Building..."
dotnet build -c Release

# Publish as single file executable for Windows
echo "Publishing for Windows..."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "./dist"

# Create assets directory and copy files
echo "Copying assets..."
mkdir -p "dist/assets"
cp "assets/icon.png" "dist/assets/"
cp "launcher-config.json" "dist/"

# Rename executable
echo "Renaming executable..."
mv "dist/GGMuLauncher.exe" "dist/Mu Launcherexe"

echo ""
echo "Build complete! Windows executable is in the 'dist' folder."

# Show file size
if [ -f "dist/Mu Launcher.exe" ]; then
    size=$(du -h "dist/Mu Launcher.exe" | cut -f1)
    echo "Executable size: $size"
fi
