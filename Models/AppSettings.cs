namespace DeepSeekTranslator.Models;

public class AppSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = "auto";
    public string TargetLanguage { get; set; } = "zh-CN";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
