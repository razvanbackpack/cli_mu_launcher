using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Net;

namespace GGMuLauncher
{
    public class LauncherConfig
    {
        public string Version { get; set; } = "1.0.0";
        public string WebsiteUrl { get; set; } = "https://example.com";
        public string updateCheckUrl { get; set; } = "https://example.com/version.json";
    }

    public class RemoteVersionInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "";
        
        [JsonPropertyName("patchUrl")]
        public string PatchUrl { get; set; } = "";
    }

    class Program
    {
        private static ConfigManager _configManager;
        private static GameLauncher _gameLauncher;
        private static LauncherConfig _launcherConfig;
        private static RemoteVersionInfo _remoteVersion;
        private static bool _updateAvailable;
        private static string _updateCheckError = "";

        static async Task Main(string[] args)
        {
            _configManager = new ConfigManager();
            _gameLauncher = new GameLauncher();
            _launcherConfig = LoadLauncherConfig();

            Console.Title = $"Mu Launcher v{_launcherConfig.Version}";
            
            // Check for updates
            await CheckForUpdates();
            
            // Set console colors for medieval theme
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();

            while (true)
            {
                ShowMainMenu();
                
                var key = Console.ReadKey(true);
                Console.Clear();

                switch (key.Key)
                {
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        if (_updateAvailable)
                            OpenPatchDownload();
                        break;
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        await LaunchGame();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        ShowSettings();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        OpenWebsite();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static void ShowMainMenu()
        {
            Console.Clear();
            
            // ASCII Art Header
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
 ███╗   ███╗██╗   ██╗
 ████╗ ████║██║   ██║
 ██╔████╔██║██║   ██║
 ██║╚██╔╝██║██║   ██║
 ██║ ╚═╝ ██║╚██████╔╝
 ╚═╝     ╚═╝ ╚═════╝ 
                      ");
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("================================");
            Console.WriteLine($"       MU ONLINE v{_launcherConfig.Version}       ");
            Console.WriteLine("================================");
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            
            // Show update option if available
            if (_updateAvailable && _remoteVersion != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"0. [!!!] NEW PATCH - MUST UPDATE (v{_remoteVersion.Version})");
                Console.WriteLine();
            }
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. [>] PLAY GAME");
            Console.WriteLine("2. [*] SETTINGS");
            Console.WriteLine("3. [@] WEBSITE");
            Console.WriteLine("4. [X] EXIT");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (_updateAvailable)
                Console.WriteLine("Press 0 to update, 1-4 for menu, or ESC to exit...");
            else
                Console.WriteLine("Press 1-4 or ESC to exit...");
            
            // Show status
            var config = _configManager.LoadConfig();
            Console.WriteLine($"Account: {(string.IsNullOrEmpty(config.AccountName) ? "Not set" : config.AccountName)}");
            Console.WriteLine($"Resolution: {config.Resolution}");
            Console.WriteLine($"Window Mode: {(config.WindowMode ? "Yes" : "No")}");
            
            // Debug update check status
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (_updateAvailable && _remoteVersion != null)
                Console.WriteLine($"Update available: v{_remoteVersion.Version}");
            else if (_remoteVersion != null)
                Console.WriteLine($"No updates (remote: v{_remoteVersion.Version}, local: v{_launcherConfig.Version})");
            else
                Console.WriteLine($"Failed to check updates: {_updateCheckError}");
        }

        static async Task LaunchGame()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">> LAUNCHING GAME");
            Console.WriteLine("=================");
            Console.WriteLine();

            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[*] Loading configuration...");
                var config = _configManager.LoadConfig();

                Console.WriteLine("[*] Applying settings...");
                await _gameLauncher.ApplySettingsAsync(config);

                Console.WriteLine("[*] Starting game...");
                await _gameLauncher.LaunchGameAsync(config);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Game launched successfully!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Error: {ex.Message}");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        static void ShowSettings()
        {
            var config = _configManager.LoadConfig();
            
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("╔════════════════════════════════════╗");
                Console.WriteLine("║           GAME SETTINGS            ║");
                Console.WriteLine("╚════════════════════════════════════╝");
                Console.WriteLine();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("┌─ Player Configuration ─────────────┐");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"│ 1. Account Name: {config.AccountName}");
                Console.WriteLine($"│ 2. Resolution: {config.Resolution}");
                Console.WriteLine($"│ 3. Window Mode: {(config.WindowMode ? "Enabled" : "Disabled")}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("├─ Audio Settings ───────────────────┤");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"│ 4. Music: {(config.SoundMusic ? "Enabled" : "Disabled")}");
                Console.WriteLine($"│ 5. Sound Effects: {(config.SoundEffect ? "Enabled" : "Disabled")}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("├─ Advanced ─────────────────────────┤");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"│ 6. Game Path: {(string.IsNullOrEmpty(config.GamePath) ? "Auto-detect" : "Custom")}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("└─────────────────────────────────────┘");
                Console.WriteLine();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("7. + Save & Return");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("8. - Cancel");
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Press 1-8 to modify settings...");
                
                var key = Console.ReadKey(true);
                
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        Console.Write("Enter account name: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        config.AccountName = Console.ReadLine() ?? "";
                        break;
                    case ConsoleKey.D2:
                        ChangeResolution(config);
                        break;
                    case ConsoleKey.D3:
                        config.WindowMode = !config.WindowMode;
                        break;
                    case ConsoleKey.D4:
                        config.SoundMusic = !config.SoundMusic;
                        break;
                    case ConsoleKey.D5:
                        config.SoundEffect = !config.SoundEffect;
                        break;
                    case ConsoleKey.D6:
                        Console.Write("Enter game path (or leave empty for auto-detect): ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        config.GamePath = Console.ReadLine() ?? "";
                        break;
                    case ConsoleKey.D7:
                        _configManager.SaveConfig(config);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Settings saved!");
                        Console.ReadKey(true);
                        return;
                    case ConsoleKey.D8:
                    case ConsoleKey.Escape:
                        return;
                }
            }
        }

        static void ChangeResolution(GameConfig config)
        {
            string[] resolutions = {
                "800x600", "1024x768", "1152x864", "1280x720",
                "1280x800", "1280x960", "1440x1080", "1600x900",
                "1600x1200", "1680x1050", "1920x1080", "1920x1200",
                "1920x1440", "2048x1536", "2560x1440", "2560x1600", "3840x2160"
            };

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════╗");
            Console.WriteLine("║        SELECT RESOLUTION           ║");
            Console.WriteLine("╚════════════════════════════════════╝");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Available Resolutions:");
            Console.WriteLine("─────────────────────");
            Console.WriteLine();

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (config.Resolution == resolutions[i])
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" > {i + 1,2}. {resolutions[i]} < CURRENT");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"   {i + 1,2}. {resolutions[i]}");
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Enter number (1-17) or ESC to cancel:");

            while (true)
            {
                var key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                
                if (key.Key >= ConsoleKey.D1 && key.Key <= ConsoleKey.D9)
                {
                    int choice = key.Key - ConsoleKey.D0;
                    if (choice >= 1 && choice <= Math.Min(9, resolutions.Length))
                    {
                        config.Resolution = resolutions[choice - 1];
                        break;
                    }
                }
                
                if (key.Key == ConsoleKey.D1 && key.Modifiers.HasFlag(ConsoleModifiers.Shift)) // '1' with shift becomes '!'
                {
                    if (10 <= resolutions.Length)
                    {
                        config.Resolution = resolutions[9]; // 10th item (index 9)
                        break;
                    }
                }
                
                // Handle 10-17 with two-digit input
                if (char.IsDigit(key.KeyChar))
                {
                    Console.Write(key.KeyChar);
                    var secondKey = Console.ReadKey();
                    if (char.IsDigit(secondKey.KeyChar))
                    {
                        string numberStr = key.KeyChar.ToString() + secondKey.KeyChar.ToString();
                        if (int.TryParse(numberStr, out int choice) && choice >= 10 && choice <= resolutions.Length)
                        {
                            config.Resolution = resolutions[choice - 1];
                            break;
                        }
                    }
                    Console.Write("\b \b\b \b"); // Clear the typed characters
                }
            }
        }

        static LauncherConfig LoadLauncherConfig()
        {
            try
            {
                if (File.Exists("launcher-config.json"))
                {
                    string json = File.ReadAllText("launcher-config.json");
                    return JsonSerializer.Deserialize<LauncherConfig>(json) ?? new LauncherConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading launcher config: {ex.Message}");
            }

            return new LauncherConfig();
        }

        static async Task CheckForUpdates()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c curl -s -L \"{_launcherConfig.updateCheckUrl}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string json = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(json))
                    {
                        json = json.Trim();
                        
                        // Debug: Show what we actually received
                        if (!json.StartsWith("{"))
                        {
                            _updateCheckError = $"Invalid response (not JSON): {json.Substring(0, Math.Min(100, json.Length))}...";
                            _updateAvailable = false;
                            return;
                        }
                        
                        _remoteVersion = JsonSerializer.Deserialize<RemoteVersionInfo>(json);
                        
                        if (_remoteVersion != null && _remoteVersion.Version != _launcherConfig.Version)
                        {
                            _updateAvailable = true;
                        }
                    }
                    else
                    {
                        _updateCheckError = $"curl failed (exit code {process.ExitCode}): {error}";
                        _updateAvailable = false;
                    }
                }
                else
                {
                    _updateCheckError = "Failed to start cmd process";
                    _updateAvailable = false;
                }
            }
            catch (Exception ex)
            {
                _updateCheckError = ex.Message;
                _updateAvailable = false;
            }
        }

        static void OpenWebsite()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(">> OPENING WEBSITE");
                Console.WriteLine("==================");
                Console.WriteLine();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[*] Opening: {_launcherConfig.WebsiteUrl}");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = _launcherConfig.WebsiteUrl,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Website opened in browser!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Error opening website: {ex.Message}");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        static void OpenPatchDownload()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(">> DOWNLOADING PATCH");
                Console.WriteLine("====================");
                Console.WriteLine();
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[!] Current version: {_launcherConfig.Version}");
                Console.WriteLine($"[!] Available version: {_remoteVersion?.Version}");
                Console.WriteLine();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[*] Opening patch download: {_remoteVersion?.PatchUrl}");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = _remoteVersion?.PatchUrl ?? "",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Patch download opened in browser!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Error opening patch download: {ex.Message}");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}