# Build Instructions

## Prerequisites

### Windows
- [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

### Linux
- [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- Ubuntu/Debian: `sudo apt install dotnet-sdk-5.0`
- Fedora: `sudo dnf install dotnet-sdk-5.0`

## Quick Build

### Linux to Windows Cross-Compilation
```bash
chmod +x build.sh
./build.sh
```

Output: `dist/GG Mu.exe` (~13MB)

## Manual Build Commands

### Windows (Native)
```cmd
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
ren "dist\GGMuLauncher.exe" "Mu Launcher.exe"
```

### Linux to Windows (Cross-Platform)
```bash
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
mv dist/GGMuLauncher.exe "dist/Mu Launcher.exe"
```

### Linux Native
```bash
dotnet restore
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
mv dist/GGMuLauncher "dist/Mu Launcher"
```

## Development Build

```bash
dotnet build -c Debug
dotnet run
```
