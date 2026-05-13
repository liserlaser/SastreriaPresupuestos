using System;
using System.IO;
using System.Text.Json;

namespace SastreriaPresupuestos.Services
{
    public class AppSettings
    {
        public string? ClientSheetExportFolder { get; set; }
        public string? QuoteExportFolder { get; set; }
        public string? OptionsExportFolder { get; set; }
    }

    public static class AppSettingsService
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SastreriaPresupuestos");

        private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return new AppSettings();

                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            Directory.CreateDirectory(SettingsDirectory);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }
    }
}
