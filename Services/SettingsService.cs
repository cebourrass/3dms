using System;
using System.IO;
using System.Text.Json;
using Analyzer.Models;

namespace Analyzer.Services
{
    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_settings.json");
        private static readonly string LayoutPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dock_layout.xml");

        public UserSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            return new UserSettings();
        }

        public void SaveSettings(UserSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public string GetLayoutPath() => LayoutPath;
    }
}
