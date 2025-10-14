
using SpecFlowSelenium.Helpers;

namespace SpecFlowLogin.Helpers.DebugTools
{
    public static class Debug
    {
        private static readonly object _lock = new();

        /// <summary>
        /// Mensajes de log con el formato:
        /// [chrome][Thread 18][12:42:03] Mensaje
        /// </summary>
        public static void Log(string message)
        {

            // if (Environment.GetEnvironmentVariable("LOG_LEVEL") == "none")
            //    return;
            // TODO la loggica para diferentes niveles de log


            // Prefijo navegador + hilo (de DriverFactory)
            string threadInfo = DriverFactory.GetThreadLabel();
            string timestamp = DateTime.Now.ToString("HH:mm:ss");

     
            string formatted = $"{threadInfo}[{timestamp}] {message}";
            lock (_lock)
            {
                Console.WriteLine(formatted);
            }
        }
    }
}
