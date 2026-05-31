using System.Windows;
using System.Windows.Threading;
using DeepSeekTranslator.Services;

namespace DeepSeekTranslator;

public partial class App : Application
{
    public App()
    {
        // 捕获所有未处理的 UI 线程异常
        DispatcherUnhandledException += (s, e) =>
        {
            Logger.Write($"Dispatcher异常: {e.Exception.GetType().Name}: {e.Exception.Message}");
            Logger.Write($"堆栈: {e.Exception.StackTrace}");
            var inner = e.Exception.InnerException;
            while (inner != null)
            {
                Logger.Write($"内部异常: {inner.GetType().Name}: {inner.Message}");
                inner = inner.InnerException;
            }
            MessageBox.Show($"未处理的异常：{e.Exception.Message}",
                "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };

        // 捕获后台线程异常
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Logger.Write($"AppDomain异常: {(ex?.GetType().Name ?? "未知")}: {(ex?.Message ?? e.ExceptionObject?.ToString())}");
            if (ex != null)
            {
                Logger.Write($"堆栈: {ex.StackTrace}");
            }
        };

        // 捕获 Task 异常
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Logger.Write($"Task异常: {e.Exception.GetType().Name}: {e.Exception.Message}");
            Logger.Write($"堆栈: {e.Exception.StackTrace}");
            e.SetObserved();
        };
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        Logger.Write("=== 应用启动 ===");
        try
        {
            var settingsService = new SettingsService();
            var settings = settingsService.Load();
            Logger.Write($"配置加载完成，IsConfigured={settings.IsConfigured}");

            if (!settings.IsConfigured)
            {
                Logger.Write("弹出 API Key 配置窗口");
                var settingsWindow = new SettingsWindow();
                var result = settingsWindow.ShowDialog();
                Logger.Write($"配置窗口返回值: {result}");

                if (result == true)
                {
                    settings.ApiKey = settingsWindow.ApiKey;
                    settings.SourceLanguage = "auto";
                    settings.TargetLanguage = "zh-CN";
                    settingsService.Save(settings);
                    Logger.Write("API Key 已保存");
                }
                else
                {
                    Logger.Write("用户取消配置，退出应用");
                    Shutdown();
                    return;
                }
            }

            Logger.Write("创建主窗口...");
            var mainWindow = new MainWindow(settings);
            mainWindow.Closed += (_, _) => Shutdown();
            Logger.Write("显示主窗口...");
            mainWindow.Show();
            Logger.Write("主窗口已显示");
        }
        catch (Exception ex)
        {
            Logger.Write($"启动异常: {ex.GetType().Name}: {ex.Message}");
            Logger.Write($"堆栈: {ex.StackTrace}");
            MessageBox.Show($"启动失败：{ex.Message}\n\n详细信息已记录到日志文件。",
                "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
