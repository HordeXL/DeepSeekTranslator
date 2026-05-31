using System.IO;

namespace DeepSeekTranslator.Services;

public static class Logger
{
    private static readonly string LogPath = Path.Combine(
        AppContext.BaseDirectory,
        "debug.log");

    public static void Write(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
        catch
        {
            // Don't crash the app if logging fails
        }
    }
}
