using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Diagnostics;

public class YamlTestFrameworkTestAdapterMessageLogger : IYamlTestFrameworkLogger
{
    public YamlTestFrameworkTestAdapterMessageLogger(IMessageLogger logger)
    {
        this.logger = logger;
    }

    public void LogVerbose(string text)
    {
        logger.SendMessage(TestMessageLevel.Informational, text);
    }

    public void LogInfo(string text)
    {
        var dt = $"{DateTime.Now}";
        logger?.SendMessage(TestMessageLevel.Informational, $"{dt}: {text}");
        File.AppendAllText(_logPath, $"{dt}: INFO: {text}\n");
    }

    public void LogWarning(string text)
    {
        var dt = $"{DateTime.Now}";
        logger?.SendMessage(TestMessageLevel.Warning, $"{dt}: {text}");
        File.AppendAllText(_logPath, $"{dt}: WARNING: {text}\n");
    }

    public void LogError(string text)
    {
        var dt = $"{DateTime.Now}";
        logger?.SendMessage(TestMessageLevel.Error, $"{dt}: {text}");
        File.AppendAllText(_logPath, $"{dt}: ERROR: {text}\n");
    }

    #region private methods and data

    private static string GetLogPath()
    {
        var pid = Process.GetCurrentProcess().Id.ToString();
        var time = DateTime.Now.ToFileTime().ToString();
        return $"log-cycodt-cli-test-framework-{time}-{pid}.log";
    }

    private IMessageLogger logger;

    private static string _logPath = GetLogPath();

    #endregion
}
