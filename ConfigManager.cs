using System;
using System.IO;
using System.Text.Json;

namespace GGMuLauncher
{
    public class GameConfig
    {
        public string AccountName { get; set; } = "";
        public string Resolution { get; set; } = "1920x1080";
        public bool WindowMode { get; set; } = true;
        public bool SoundMusic { get; set; } = true;
        public bool SoundEffect { get; set; } = true;
        public string GamePath { get; set; } = "";
    }

    public class ConfigManager
    {
        private readonly string _configPath;

        public ConfigManager()
        {
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ggmu-launcher-config.json");
        }

        public GameConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize<GameConfig>(json) ?? new GameConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
            }

            return new GameConfig();
        }

        public bool SaveConfig(GameConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
                return false;
            }
        }
    }
}