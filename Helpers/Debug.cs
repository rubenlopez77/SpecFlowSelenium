
namespace SpecFlowLogin.Helpers.DebugTools
{
    static class Debug
    {
        //TODO: Implementar diferentes log levels (info, debug, none, etc.)
        public static void Log(string message)
        {
           // if (Environment.GetEnvironmentVariable("LOG_LEVEL") == "none")
            //    return;

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Thread {System.Threading.Thread.CurrentThread.ManagedThreadId}] {message}");
        }
    }
}
