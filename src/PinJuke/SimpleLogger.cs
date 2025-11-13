using System;
using System.IO;

namespace PinJuke
{
    public static class SimpleLogger
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PinJuke_Debug.log");
        
        static SimpleLogger()
        {
            // Clear the log file at startup
            try
            {
                File.WriteAllText(logFilePath, $"=== PinJuke Debug Log Started at {DateTime.Now} ===\n");
                Console.WriteLine($"Debug log will be written to: {logFilePath}");
            }
            catch
            {
                // Ignore errors creating log file
            }
        }
        
        public static void Log(string message)
        {
            try
            {
                var timestamped = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                Console.WriteLine(timestamped); // Still write to console
                File.AppendAllText(logFilePath, timestamped + "\n");
            }
            catch
            {
                // Ignore errors writing to log
            }
        }
    }
}
