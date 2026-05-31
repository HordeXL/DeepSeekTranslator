using System.Windows;
using System.Windows.Controls;

namespace DeepSeekTranslator;

public partial class SettingsWindow : Window
{
    public string ApiKey { get; private set; } = string.Empty;

    public SettingsWindow()
    {
        InitializeComponent();
        ApiKeyBox.Focus();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        ApiKey = ApiKeyBox.Password.Trim();
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            MessageBox.Show("请输入有效的 API Key。", "提示", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
