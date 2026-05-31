namespace DeepSeekTranslator.Models;

public class LanguageInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;

    public string DisplayName => $"{NativeName} ({Name})";

    public static List<LanguageInfo> GetSupportedLanguages() =>
    [
        new() { Code = "auto", Name = "Auto Detect", NativeName = "自动检测" },
        new() { Code = "zh-CN", Name = "Chinese (Simplified)", NativeName = "简体中文" },
        new() { Code = "en", Name = "English", NativeName = "英文" },
        new() { Code = "ja", Name = "Japanese", NativeName = "日文" },
        new() { Code = "ko", Name = "Korean", NativeName = "韩文" },
        new() { Code = "fr", Name = "French", NativeName = "法文" },
        new() { Code = "de", Name = "German", NativeName = "德文" },
        new() { Code = "es", Name = "Spanish", NativeName = "西班牙文" },
        new() { Code = "ru", Name = "Russian", NativeName = "俄文" },
        new() { Code = "pt", Name = "Portuguese", NativeName = "葡萄牙文" },
        new() { Code = "it", Name = "Italian", NativeName = "意大利文" },
        new() { Code = "nl", Name = "Dutch", NativeName = "荷兰文" },
        new() { Code = "ar", Name = "Arabic", NativeName = "阿拉伯文" },
        new() { Code = "tr", Name = "Turkish", NativeName = "土耳其文" },
        new() { Code = "vi", Name = "Vietnamese", NativeName = "越南文" },
        new() { Code = "th", Name = "Thai", NativeName = "泰文" },
        new() { Code = "pl", Name = "Polish", NativeName = "波兰文" },
        new() { Code = "uk", Name = "Ukrainian", NativeName = "乌克兰文" },
    ];
}
