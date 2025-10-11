using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using TechTalk.SpecFlow;
using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools; 
using DotNetEnv; 

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Gestiona el ciclo de vida del IWebDriver por escenario de SpecFlow.
    ///
    /// - Modo PARALLEL: un solo navegador por escenario (el primero de BROWSERS).
    /// - Modo MULTI: crea un MultiDriver que replica acciones en todos los navegadores indicados.
    ///
    /// Robustece la paralelización:
    /// - Perfiles aislados: /tmp/wd-profiles/{browser}/{ScenarioHash}/profile-{GUID}
    /// - Fail-fast: si falla la creación del driver, aborta el escenario con excepción clara.
    /// - Limpieza: cierra y libera todos los drivers al finalizar el escenario.
    /// </summary>
    [Binding, Parallelizable(ParallelScope.All)]
    public class DriverFactory
    {
        private static readonly ThreadLocal<IWebDriver?> _currentDriver = new();
        public static IWebDriver CurrentDriver
        {
            get => _currentDriver.Value ?? throw new InvalidOperationException("ERROR: CurrentDriver no inicializado.");
            private set => _currentDriver.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private readonly ScenarioContext _context;
        private readonly IObjectContainer _container;

        public DriverFactory(ScenarioContext context, IObjectContainer container)
        {
            _context = context;
            _container = container;
        }

        // Carga .env una vez al iniciar la test run (no falla si no existe)
        [BeforeTestRun(Order = -1000)]
        public static void BeforeTestRun()
        {
            try
            {
                Env.Load();
                Debug.Log("Entorno: .env cargado.");
            }
            catch (Exception ex)
            {
                Debug.Log($"⚠️ No se pudo cargar .env: {ex.Message}");
            }
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            var mode = (Environment.GetEnvironmentVariable("EXECUTION_MODE") ?? "PARALLEL")
                .Trim().ToUpperInvariant();

            var browsersEnv = Environment.GetEnvironmentVariable("BROWSERS") ?? "chrome";
            var browsers = browsersEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(b => b.ToLowerInvariant())
                .ToArray();

            bool headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "true")
                .Trim().ToLowerInvariant() == "true";

            Debug.Log($"Escenario: '{_context.ScenarioInfo.Title}'  |  Modo: {mode}  |  Browsers: {string.Join(", ", browsers)}  |  headless={headless}");

            if (mode == "MULTI")
            {
                // Lanza TODOS los navegadores y crea el proxy MultiDriver
                var drivers = new ConcurrentDictionary<string, IWebDriver>();

                Parallel.ForEach(browsers, browser =>
                {
                    var d = CreateDriver(browser, headless);
                    drivers[browser] = d;
                    Debug.Log($"{browser} inicializado para MULTI.");
                });

                var multi = new MultiDriver(drivers); // Asegúrate de no tener otra MultiNavigation duplicada (ver nota arriba)
                CurrentDriver = multi;

                // Guarda para cleanup
                _context["drivers"] = drivers.Values.ToList();

                // Inyección para Steps/Pages
                _container.RegisterInstanceAs(new DriverContext(CurrentDriver));

                Debug.Log("MultiDriver activo (espejo cross-browser).");
            }
            else
            {
                // Un solo navegador por escenario (el primero de la lista)
                var browser = browsers.First();
                var driver = CreateDriver(browser, headless);
                CurrentDriver = driver;

                _context["drivers"] = new List<IWebDriver> { driver };
                _container.RegisterInstanceAs(new DriverContext(CurrentDriver));

                Debug.Log($"Driver '{browser}' listo (PARALLEL).");
            }
        }

        [AfterScenario(Order = 0)]
        public void AfterScenario()
        {
            try
            {
                if (_context.TryGetValue("drivers", out List<IWebDriver> drivers) && drivers != null)
                {
                    Debug.Log($"Cerrando {drivers.Count} driver(s)...");
                    Parallel.ForEach(drivers, d =>
                    {
                        try
                        {
                            d.Quit();
                            d.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"⚠️ Error al cerrar driver: {ex.Message}");
                        }
                    });
                }
            }
            finally
            {
                _currentDriver.Value = null; // rompe la referencia del ThreadLocal
                Debug.Log("Limpieza completada.");
            }
        }

        [AfterTestRun(Order = 999)]
        public static void AfterTestRun()
        {
            try
            {
                _currentDriver.Dispose(); // libera estructuras del ThreadLocal
                Debug.Log("ThreadLocal<IWebDriver> liberado tras la test run.");
            }
            catch (Exception ex)
            {
                Debug.Log($"⚠️ Error liberando ThreadLocal: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea el driver con aislamiento de perfil por navegador + escenario + GUID.
        /// Fail-fast: si falla la creación, aborta con InvalidOperationException.
        /// </summary>
        private IWebDriver CreateDriver(string browserName, bool headless)
        {
            browserName = browserName.ToLowerInvariant().Trim();

            // Aislamiento total de perfiles temporales
            //string scenarioName = _context?.ScenarioInfo?.Title ?? "unknown";
            //int scenarioHash = scenarioName.GetHashCode();
            //string profile = Path.Combine(
            //    Path.GetTempPath(),
            //    "wd-profiles",
            //    browserName,
            //    scenarioHash.ToString(),
            //    $"profile-{Guid.NewGuid()}"
            //);
            //Directory.CreateDirectory(Path.GetDirectoryName(profile)!);

            try
            {
                switch (browserName)
                {
                    case "firefox":
                        {
                            var fopts = new FirefoxOptions();
                            if (headless) fopts.AddArgument("--headless");
                            // Firefox maneja su propio perfil; no usamos user-data-dir aquí.
                            return new FirefoxDriver(fopts);
                        }

                    case "edge":
                        {
                            var eopts = new EdgeOptions();
                            //eopts.AddArgument($"--user-data-dir={profile}");
                            if (headless) eopts.AddArgument("--headless=new");
                            eopts.AddArgument("--no-sandbox");
                            eopts.AddArgument("--disable-dev-shm-usage");
                            return new EdgeDriver(eopts);
                        }

                    case "chrome":
                    default:
                        {
                            var copts = new ChromeOptions();
                            //copts.AddArgument($"--user-data-dir={profile}");
                            if (headless) copts.AddArgument("--headless=new");
                            copts.AddArgument("--no-sandbox");
                            copts.AddArgument("--disable-dev-shm-usage");
                            copts.AddArgument("--disable-gpu");
                            return new ChromeDriver(copts);
                        }
                }
            }
            catch (Exception ex)
            {
                string msg = $"❌ No se pudo iniciar el navegador '{browserName}'. Se aborta el escenario.\n{ex.Message}";
                Debug.Log(msg);
                throw new InvalidOperationException(msg, ex); // FAIL-FAST
            }
        }
    }
}
