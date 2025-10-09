
namespace SpecFlowLogin.Helpers.DebugTools
{
    static class Debug
    {
        private static readonly string? log_level = Environment.GetEnvironmentVariable("LOG_LEVEL");

        public static void Log(string message)
        {
            if (log_level=="no")
                return;

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Thread {System.Threading.Thread.CurrentThread.ManagedThreadId}] {message}");
        }
    }
}
