using WebBanNuocMVC.DesignPatterns.Singleton;

namespace WebBanNuocMVC.DesignPatterns.Singleton
{
    public class LoggerService : ILoggerService
    {
        private readonly string path = "logs.txt";

        public void LogInfo(string message)
        {
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] INFO: {message}");
            File.AppendAllText(path, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] INFO: {message}{Environment.NewLine}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] WARN: {message}");
            File.AppendAllText(path, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] WARN: {message}{Environment.NewLine}");
        }

        public void LogError(string message, Exception? ex = null)
        {
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] ERROR: {message}");
            File.AppendAllText(path, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] ERROR: {message}{Environment.NewLine}");

            if (ex != null)
            {
                File.AppendAllText(path, $"Exception: {ex}{Environment.NewLine}");
            }
        }
    }
}