using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GGMuLauncher
{
    public class GameLauncher
    {
        private readonly Dictionary<string, int> _resolutionMap = new Dictionary<string, int>
        {
            {"800x600", 0}, {"1024x768", 1}, {"1152x864", 2}, {"1280x720", 3},
            {"1280x800", 4}, {"1280x960", 5}, {"1440x1080", 6}, {"1600x900", 7},
            {"1600x1200", 8}, {"1680x1050", 9}, {"1920x1080", 10}, {"1920x1200", 11},
            {"1920x1440", 12}, {"2048x1536", 13}, {"2560x1440", 14}, {"2560x1600", 15},
            {"3840x2160", 16}
        };

        public async Task ApplySettingsAsync(GameConfig config)
        {
            await Task.Run(() =>
            {
                try
                {
                    ApplyRegistrySettings(config);
                    CreateLauncherOptionFile(config);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to apply settings: {ex.Message}");
                }
            });
        }

        public async Task LaunchGameAsync(GameConfig config)
        {
            await Task.Run(() =>
            {
                string gamePath = config.GamePath;

                // If no game path specified, try to find main.exe in current directory
                if (string.IsNullOrEmpty(gamePath))
                {
                    string defaultPath = Path.Combine(Environment.CurrentDirectory, "main.exe");
                    if (File.Exists(defaultPath))
                    {
                        gamePath = defaultPath;
                    }
                    else
                    {
                        throw new Exception("Game executable not found. Please place main.exe in the same folder as the launcher, or configure the game path in settings.");
                    }
                }

                if (!File.Exists(gamePath))
                {
                    throw new Exception($"Game executable not found at: {gamePath}");
                }

                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = gamePath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(gamePath)
                    };

                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to launch game: {ex.Message}");
                }
            });
        }

        private void ApplyRegistrySettings(GameConfig config)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\Webzen\Mu\Config");
                if (key != null)
                {
                    int resolutionIndex = _resolutionMap.GetValueOrDefault(config.Resolution, 10);
                    int musicValue = config.SoundMusic ? 1 : 0;
                    int effectValue = config.SoundEffect ? 1 : 0;
                    int windowValue = config.WindowMode ? 1 : 0;

                    key.SetValue("MusicOnOFF", musicValue, RegistryValueKind.DWord);
                    key.SetValue("SoundOnOFF", effectValue, RegistryValueKind.DWord);
                    key.SetValue("WindowMode", windowValue, RegistryValueKind.DWord);
                    key.SetValue("Resolution", resolutionIndex, RegistryValueKind.DWord);
                    key.SetValue("ID", config.AccountName ?? "", RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                // Registry access might fail on some systems, but continue anyway
                System.Diagnostics.Debug.WriteLine($"Registry update failed: {ex.Message}");
            }
        }

        private void CreateLauncherOptionFile(GameConfig config)
        {
            try
            {
                int resolutionIndex = _resolutionMap.GetValueOrDefault(config.Resolution, 10);
                int windowMode = config.WindowMode ? 1 : 0;
                string accountName = config.AccountName ?? "";

                string content = $"DevModeIndex:{resolutionIndex}\r\nWindowMode:{windowMode}\r\nID:{accountName}\r\n";
                string filePath = Path.Combine(Environment.CurrentDirectory, "LauncherOption.if");

                File.WriteAllText(filePath, content);
                System.Diagnostics.Debug.WriteLine("LauncherOption.if file created successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create LauncherOption.if: {ex.Message}");
                // Don't throw here as this is optional
            }
        }
    }
}