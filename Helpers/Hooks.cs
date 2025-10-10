using TechTalk.SpecFlow;
using DotNetEnv;
using System;
using System.Linq;
using SpecFlowLogin.Helpers.DebugTools;

namespace SpecFlowSelenium.Helpers
{
    [Binding]
    public sealed class Hooks
    {
        [BeforeTestRun(Order = 0)]
        public static void LoadEnvironment()
        {
            try
            {
                // Cargar variables desde el .env
                Env.Load();
                Debug.Log ("Variables .env cargadas correctamente");
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: No se encontró o no se pudo leer el archivo .env ({ex.Message})");
                throw;
            }

            // Verificar que las variables críticas existan
            string[] requiredVars = new[] { "BASE_URL", "BROWSERS", "HEADLESS" };
            var missing = requiredVars.Where(v => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v))).ToList();

            if (missing.Any())
            {
                Debug.Log($"⚠️  Faltan variables de entorno obligatorias: {string.Join(", ", missing)}");
                throw new InvalidOperationException("Variables .env incompletas. Corrige antes de ejecutar las pruebas.");
            }

            // Mostrar resumen cargado
            Debug.Log("Entorno cargado:");
            Debug.Log($"BASE_URL = {Environment.GetEnvironmentVariable("BASE_URL")}");
            Debug.Log($"BROWSERS = {Environment.GetEnvironmentVariable("BROWSERS")}");
            Debug.Log($"HEADLESS = {Environment.GetEnvironmentVariable("HEADLESS")}");
        }

        [AfterTestRun(Order = 0)]
        public static void AfterAllTests()
        {
            Console.WriteLine("Finalizando pruebas...");
           
        }
    }
}
