using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekTranslator.Services;

public class DeepSeekService
{
    private readonly HttpClient _http;
    private string _apiKey;

    public DeepSeekService(string apiKey)
    {
        _apiKey = apiKey;
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.deepseek.com"),
            Timeout = TimeSpan.FromSeconds(60)
        };
    }

    public void UpdateApiKey(string apiKey) => _apiKey = apiKey;

    public async Task<string> TranslateAsync(string text, string sourceLang, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var sourceDesc = sourceLang == "auto" ? "自动检测" : GetLanguageName(sourceLang);

        string systemPrompt = $"You are a professional translator. Translate the following text from {sourceDesc} to {GetLanguageName(targetLang)}. " +
                              "Return ONLY the translated text, nothing else. No explanations, no notes.";

        string userPrompt = sourceLang == "auto"
            ? $"Please translate the following text to {GetLanguageName(targetLang)}:\n\n{text}"
            : $"Please translate the following text from {GetLanguageName(sourceLang)} to {GetLanguageName(targetLang)}:\n\n{text}";

        var request = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.3,
            max_tokens = 8192,
            stream = false
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DeepSeekResponse>();
        return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty;
    }

    private static string GetLanguageName(string code) => code switch
    {
        "zh-CN" => "简体中文",
        "en" => "English",
        "ja" => "日本語",
        "ko" => "한국어",
        "fr" => "Français",
        "de" => "Deutsch",
        "es" => "Español",
        "ru" => "Русский",
        "pt" => "Português",
        "it" => "Italiano",
        "nl" => "Nederlands",
        "ar" => "العربية",
        "tr" => "Türkçe",
        "vi" => "Tiếng Việt",
        "th" => "ไทย",
        "pl" => "Polski",
        "uk" => "Українська",
        _ => "English"
    };

    private class DeepSeekResponse
    {
        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
