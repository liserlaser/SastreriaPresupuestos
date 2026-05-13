using System;
using System.IO;
using System.Text.Json;

namespace SastreriaPresupuestos.Services;

public class ExportFolderSettings
{
    public string ClientSheetFolder { get; set; } = string.Empty;
    public string QuoteFolder { get; set; } = string.Empty;
    public string OptionsFolder { get; set; } = string.Empty;
}

public static class ExportFolderSettingsService
{
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SastreriaPresupuestos", "export-folders.json");

    public static ExportFolderSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new ExportFolderSettings();

        try
        {
            return JsonSerializer.Deserialize<ExportFolderSettings>(File.ReadAllText(SettingsPath)) ?? new ExportFolderSettings();
        }
        catch
        {
            return new ExportFolderSettings();
        }
    }

    public static void Save(ExportFolderSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }
}
