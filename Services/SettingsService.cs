using System.IO;
using System.Text.Json;
using DeepSeekTranslator.Models;

namespace DeepSeekTranslator.Services;

public class SettingsService
{
    private static readonly string ConfigPath = Path.Combine(
        AppContext.BaseDirectory,
        "apikey.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return new AppSettings();

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }
}
