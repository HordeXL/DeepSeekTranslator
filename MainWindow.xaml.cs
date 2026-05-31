using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using DeepSeekTranslator.Models;
using DeepSeekTranslator.Services;

namespace DeepSeekTranslator;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly DeepSeekService _deepSeekService;
    private readonly SettingsService _settingsService = new();
    private bool _isDarkTheme = true;
    private bool _isTranslating;

    public MainWindow(AppSettings settings)
    {
        try
        {
            Logger.Write("MainWindow: 开始初始化...");
            InitializeComponent();
            Logger.Write("MainWindow: InitializeComponent 完成");

            _settings = settings;
            _deepSeekService = new DeepSeekService(_settings.ApiKey);
            Logger.Write("MainWindow: DeepSeekService 创建完成");

            var languages = LanguageInfo.GetSupportedLanguages();
            SourceLangCombo.ItemsSource = languages;
            TargetLangCombo.ItemsSource = languages.Where(l => l.Code != "auto").ToList();
            Logger.Write("MainWindow: 语言列表加载完成");

            SourceLangCombo.SelectedValue = _settings.SourceLanguage;
            TargetLangCombo.SelectedValue = _settings.TargetLanguage;
            Logger.Write($"MainWindow: 语言偏好设置完成 (src={_settings.SourceLanguage}, tgt={_settings.TargetLanguage})");

            StatusText.Text = "就绪";
            Logger.Write("MainWindow: 初始化完成");
        }
        catch (Exception ex)
        {
            Logger.Write($"MainWindow 构造异常: {ex.GetType().Name}: {ex.Message}");
            Logger.Write($"堆栈: {ex.StackTrace}");
            MessageBox.Show($"主窗口初始化失败：{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Logger.Write("MainWindow.Loaded 事件触发");
    }

    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Logger.Write($"MainWindow.Closing 事件触发");
    }

    private void OnLanguageChanged(object sender, RoutedEventArgs e)
    {
        if (SourceLangCombo.SelectedValue?.ToString() == "auto")
            return;

        if (!string.IsNullOrWhiteSpace(SourceTextBox?.Text))
            _ = PerformTranslationAsync();
    }

    private void OnSourceTextChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            var text = SourceTextBox.Text;
            CharCountText.Text = $"{text.Length} 字符";
            TranslateButton.IsEnabled = !string.IsNullOrWhiteSpace(text) && !_isTranslating;

            if (string.IsNullOrWhiteSpace(text))
            {
                ResultTextBox.Text = "等待翻译...";
                try { ResultTextBox.Foreground = (Brush)FindResource("SecondaryTextBrush"); }
                catch { }
            }
        }
        catch { }
    }

    private async void OnTranslate(object sender, RoutedEventArgs e)
    {
        await PerformTranslationAsync();
    }

    private async Task PerformTranslationAsync()
    {
        if (_isTranslating) return;

        var text = SourceTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!_settings.IsConfigured)
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() != true)
                return;
            _settings.ApiKey = settingsWindow.ApiKey;
            _deepSeekService.UpdateApiKey(settingsWindow.ApiKey);
            _settingsService.Save(_settings);
            Logger.Write("API Key 已配置");
        }

        _isTranslating = true;
        TranslateButton.IsEnabled = false;
        TranslateButton.Content = "⏳ 翻译中...";
        StatusText.Text = "正在翻译...";

        try
        {
            var sourceLang = SourceLangCombo.SelectedValue?.ToString() ?? "auto";
            var targetLang = TargetLangCombo.SelectedValue?.ToString() ?? "zh-CN";

            _settings.SourceLanguage = sourceLang;
            _settings.TargetLanguage = targetLang;
            _settingsService.Save(_settings);

            Logger.Write($"开始翻译: [{sourceLang}] -> [{targetLang}], 文本长度={text.Length}");
            var result = await _deepSeekService.TranslateAsync(text, sourceLang, targetLang);
            Logger.Write($"翻译完成, 结果长度={result?.Length ?? 0}");

            if (!string.IsNullOrWhiteSpace(result))
            {
                ResultTextBox.Text = result;
                try { ResultTextBox.Foreground = (Brush)FindResource("PrimaryTextBrush"); }
                catch { }
                StatusText.Text = "翻译完成 ✓";
            }
            else
            {
                ResultTextBox.Text = "翻译失败，请重试。";
                try { ResultTextBox.Foreground = (Brush)FindResource("ErrorBrush"); }
                catch { }
                StatusText.Text = "翻译失败";
            }
        }
        catch (HttpRequestException ex)
        {
            Logger.Write($"网络错误: {ex.Message}");
            ResultTextBox.Text = $"网络错误：{ex.Message}";
            try { ResultTextBox.Foreground = (Brush)FindResource("ErrorBrush"); }
            catch { }
            StatusText.Text = "网络错误";
        }
        catch (Exception ex)
        {
            Logger.Write($"翻译异常: {ex.GetType().Name}: {ex.Message}");
            ResultTextBox.Text = $"错误：{ex.Message}";
            try { ResultTextBox.Foreground = (Brush)FindResource("ErrorBrush"); }
            catch { }
            StatusText.Text = "发生错误";
        }
        finally
        {
            _isTranslating = false;
            TranslateButton.IsEnabled = true;
            TranslateButton.Content = "🔄 翻译";
        }
    }

    private void OnSwapLanguages(object sender, RoutedEventArgs e)
    {
        if (SourceLangCombo.SelectedValue?.ToString() == "auto")
        {
            MessageBox.Show("自动检测模式不能作为目标语言，请先选择具体的源语言。", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var temp = SourceLangCombo.SelectedValue;
        SourceLangCombo.SelectedValue = TargetLangCombo.SelectedValue;
        TargetLangCombo.SelectedValue = temp;

        var resultText = ResultTextBox.Text;
        if (!string.IsNullOrWhiteSpace(resultText)
            && resultText != "等待翻译..."
            && resultText != "翻译失败，请重试。")
        {
            SourceTextBox.Text = resultText;
            ResultTextBox.Text = "等待翻译...";
            try { ResultTextBox.Foreground = (Brush)FindResource("SecondaryTextBrush"); }
            catch { }
            _ = PerformTranslationAsync();
        }
    }

    private void OnCopyResult(object sender, RoutedEventArgs e)
    {
        var text = ResultTextBox.Text;
        if (!string.IsNullOrWhiteSpace(text)
            && text != "等待翻译..."
            && text != "翻译失败，请重试。")
        {
            Clipboard.SetText(text);
            StatusText.Text = "已复制到剪贴板 ✓";
        }
    }

    private void OnClear(object sender, RoutedEventArgs e)
    {
        SourceTextBox.Clear();
        ResultTextBox.Text = "等待翻译...";
        try { ResultTextBox.Foreground = (Brush)FindResource("SecondaryTextBrush"); }
        catch { }
        StatusText.Text = "已清空";
    }

    private void OnOpenSettings(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                _settings.ApiKey = settingsWindow.ApiKey;
                _deepSeekService.UpdateApiKey(settingsWindow.ApiKey);
                _settingsService.Save(_settings);
                StatusText.Text = "API Key 已更新 ✓";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"设置保存失败：{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        try
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme(_isDarkTheme);
        }
        catch (Exception ex)
        {
            Logger.Write($"主题切换异常: {ex.Message}");
        }
    }

    private void ApplyTheme(bool isDark)
    {
        var appResources = Application.Current.Resources.MergedDictionaries;
        appResources.Clear();
        var uri = isDark ? "Styles.xaml" : "LightStyles.xaml";
        appResources.Add(new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) });

        ThemeButton.Content = isDark ? "🌙" : "☀️";
        _isDarkTheme = isDark;
    }
}
