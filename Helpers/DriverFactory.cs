using BoDi;
using DotNetEnv;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using SpecFlowLogin.Helpers.DebugTools;
using System.Collections.Concurrent;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Helpers
{
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
        private static readonly object _lockObject = new object();

        public DriverFactory(ScenarioContext context, IObjectContainer container)
        {
            _context = context;
            _container = container;
        }

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

            Debug.Log($"Escenario: '{_context.ScenarioInfo.Title}' | Modo: {mode} | Browsers: {string.Join(", ", browsers)} | headless={headless}");

            if (mode == "MULTI")
            {
                // CREACIÓN SECUENCIAL para evitar conflictos en CI
                var drivers = new ConcurrentDictionary<string, IWebDriver>();

                foreach (var browser in browsers)
                {
                    try
                    {
                        var d = CreateDriver(browser, headless);
                        drivers[browser] = d;
                        Debug.Log($"{browser} inicializado para MULTI.");

                        // Pequeña pausa entre creación de drivers
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"❌ Error creando {browser}: {ex.Message}");
                        // Continúa con los otros navegadores en lugar de fallar completamente
                        continue;
                    }
                }

                if (drivers.IsEmpty)
                {
                    throw new InvalidOperationException("No se pudo crear ningún driver para el modo MULTI.");
                }

                var multi = new MultiDriver(drivers);
                CurrentDriver = multi;
                _context["drivers"] = drivers.Values.ToList();
                _container.RegisterInstanceAs(new DriverContext(CurrentDriver));
                Debug.Log($"MultiDriver activo con {drivers.Count} navegadores.");
            }
            else
            {
                // Modo PARALLEL - solo el primer navegador
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

                    // Cierre secuencial para mayor estabilidad
                    foreach (var driver in drivers)
                    {
                        try
                        {
                            driver?.Quit();
                            driver?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"⚠️ Error al cerrar driver: {ex.Message}");
                        }
                    }
                }
            }
            finally
            {
                _currentDriver.Value = null;
                Debug.Log("Limpieza completada.");
            }
        }

        [AfterTestRun(Order = 999)]
        public static void AfterTestRun()
        {
            try
            {
                _currentDriver.Dispose();
                Debug.Log("ThreadLocal<IWebDriver> liberado tras la test run.");
            }
            catch (Exception ex)
            {
                Debug.Log($"⚠️ Error liberando ThreadLocal: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea el driver con directorio de perfil único y opciones optimizadas para CI
        /// </summary>
        private IWebDriver CreateDriver(string browserName, bool headless)
        {
            browserName = browserName.ToLowerInvariant().Trim();

            // Directorio de perfil único por thread y tiempo
            string profileDir = Path.Combine(
                Path.GetTempPath(),
                "wd-profiles",
                $"{browserName}-{Thread.CurrentThread.ManagedThreadId}-{DateTime.Now:HHmmssfff}"
            );

            try
            {
                switch (browserName)
                {
                    case "firefox":
                        {
                            var fopts = new FirefoxOptions();
                            if (headless)
                                fopts.AddArgument("--headless");

                            // Opciones específicas para CI
                            fopts.AddArgument("--no-sandbox");
                            fopts.AddArgument("--disable-dev-shm-usage");
                            fopts.AddArgument("--disable-gpu");
                            fopts.SetPreference("dom.ipc.processCount", 1);
                            fopts.AcceptInsecureCertificates = true;

                            // Timeouts aumentados para CI
                            var service = FirefoxDriverService.CreateDefaultService();
                            service.HideCommandPromptWindow = true;

                            return new FirefoxDriver(service, fopts, TimeSpan.FromSeconds(60));
                        }
                    //case "edge":
                    //    {
                    //        var eopts = new EdgeOptions();

                    //        // Directorio de perfil único
                    //        eopts.AddArgument($"--user-data-dir={profileDir}");
                    //        if (headless)
                    //            eopts.AddArgument("--headless=new");

                    //        // Opciones para CI
                    //        eopts.AddArgument("--no-sandbox");
                    //        eopts.AddArgument("--disable-dev-shm-usage");
                    //        eopts.AddArgument("--disable-gpu");
                    //        eopts.AddArgument("--disable-extensions");
                    //        eopts.AddArgument("--remote-debugging-port=0");
                    //        eopts.AcceptInsecureCertificates = true;

                    //        var service = EdgeDriverService.CreateDefaultService();
                    //        service.HideCommandPromptWindow = true;

                    //        return new EdgeDriver(service, eopts, TimeSpan.FromSeconds(60));
                    //    }
                    case "chrome":
                    default:
                        {
                            var copts = new ChromeOptions();

                            // Directorio de perfil único
                            copts.AddArgument($"--user-data-dir={profileDir}");
                            if (headless)
                                copts.AddArgument("--headless=new");

                            // Opciones optimizadas para CI
                            copts.AddArgument("--no-sandbox");
                            copts.AddArgument("--disable-dev-shm-usage");
                            copts.AddArgument("--disable-gpu");
                            copts.AddArgument("--disable-extensions");
                            copts.AddArgument("--remote-debugging-port=0");
                            copts.AddArgument("--no-first-run");
                            copts.AddArgument("--disable-background-timer-throttling");
                            copts.AddArgument("--disable-backgrounding-occluded-windows");
                            copts.AddArgument("--disable-renderer-backgrounding");
                            copts.AcceptInsecureCertificates = true;

                            var service = ChromeDriverService.CreateDefaultService();
                            service.HideCommandPromptWindow = true;

                            return new ChromeDriver(service, copts, TimeSpan.FromSeconds(60));
                        }
                }
            }
            catch (Exception ex)
            {
                string msg = $"❌ No se pudo iniciar el navegador '{browserName}'. Se aborta el escenario.\n{ex.Message}";
                Debug.Log(msg);

                // Limpiar directorio temporal si existe
                try { Directory.Delete(profileDir, true); } catch { }

                throw new InvalidOperationException(msg, ex);
            }
        }
    }
}