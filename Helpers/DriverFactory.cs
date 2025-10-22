using NUnit.Framework;



using TechTalk.SpecFlow;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using DotNetEnv;

using SpecFlowLogin.Helpers.DebugTools;


// 🔹 Atributos de configuración global de NUnit
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(6)]

namespace SpecFlowSelenium.Helpers
{
    [Binding, Parallelizable(ParallelScope.Fixtures)]
    public class DriverFactory
    {
        private static readonly ThreadLocal<IWebDriver?> _currentDriver = new();
        private static readonly ThreadLocal<string?> _currentBrowser = new();
        private static readonly ThreadLocal<ScenarioContext?> _currentContext = new();

        public static IWebDriver CurrentDriver =>
            _currentDriver.Value ?? throw new InvalidOperationException("ERROR: CurrentDriver no inicializado.");

        public DriverFactory(ScenarioContext context)
        {
            _currentContext.Value = context;
        }

        [BeforeTestRun(Order = 0)]
        public static void LoadEnv()
        {
            try { Env.Load(); }
            catch { Console.WriteLine("No se encontró .env (continuamos con variables de entorno CI)"); }
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            var context = _currentContext.Value ?? throw new InvalidOperationException("ScenarioContext no disponible en BeforeScenario");
        
        // 1️Detectar modo pipeline matrix
        string? singleBrowser = Environment.GetEnvironmentVariable("BROWSER");
        string[] browsers;

        if (!string.IsNullOrWhiteSpace(singleBrowser))
        {
            // CI: ejecutando un solo navegador (matrix)
            browsers = new[] { singleBrowser.Trim().ToLowerInvariant() };
            Debug.Log($"[CI MODE] Ejecutando solo en navegador: {browsers[0]}");
        }
        else
        {
            // Local / manual: varios navegadores definidos
            var browsersEnv = Environment.GetEnvironmentVariable("BROWSERS") ?? "chrome";
            browsers = browsersEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(b => b.ToLowerInvariant())
                .ToArray();
            Debug.Log($"[LOCAL MODE] Navegadores detectados: {string.Join(", ", browsers)}");
        }


            bool headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "true")
                .Trim().ToLowerInvariant() == "true";

            var execMode = (Environment.GetEnvironmentVariable("EXECUTION_MODE") ?? "PARALLEL")
                .Trim().ToUpperInvariant();

            Debug.Log($"Escenario: '{context.ScenarioInfo.Title}'  |  EXECUTION_MODE={execMode}  |  headless={headless}");

            // 2️⃣ Crear drivers según modo de ejecución
            var drivers = new List<IWebDriver>();

            if (execMode == "PARALLEL")
            {
                Debug.Log($"[PARALLEL MODE] Creando drivers en paralelo: {string.Join(", ", browsers)}");

                // 🔹 Lanza la creación de cada driver en su propio hilo
                var driverTasks = browsers.Select(browser =>
                    Task.Run(() =>
                    {
                        var driver = CreateDriver(browser, headless);
                        lock (drivers)
                        {
                            drivers.Add(driver);
                        }
                        Debug.Log($"[{browser}][Thread {Thread.CurrentThread.ManagedThreadId}] Driver iniciado (headless={headless})");
                        return driver;
                    })
                );

                // 🔹 Espera a que todos terminen
                Task.WhenAll(driverTasks).GetAwaiter().GetResult();
            }
            else
            {
                Debug.Log($"[{execMode} MODE] Creando drivers en secuencia: {string.Join(", ", browsers)}");

                // 🔹 Secuencial (modo MULTI o SINGLE)
                foreach (var browser in browsers)
                {
                    var driver = CreateDriver(browser, headless);
                    drivers.Add(driver);
                    Debug.Log($"[{browser}][Thread {Thread.CurrentThread.ManagedThreadId}] Driver iniciado (headless={headless})");
                }
            }

            // Guardar drivers en el contexto del escenario
            context["drivers"] = drivers;
            _currentDriver.Value = drivers.First();
            _currentBrowser.Value = browsers.First();

            Debug.Log($"Driver inicializado para '{_currentBrowser.Value}' (headless={headless})");
        }


        [AfterScenario(Order = 0)]
        public void AfterScenario()
        {
            var context = _currentContext.Value;
            if (context != null && context.TryGetValue("drivers", out List<IWebDriver> drivers))
            {
                foreach (var driver in drivers)
                {
                    try
                    {
                        Debug.Log($"{GetThreadLabel()} Cerrando navegador...");
                        driver.Quit();
                        driver.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"Error cerrando driver: {ex.Message}");
                    }
                }
            }

            _currentDriver.Value = null;
            _currentBrowser.Value = null;
            _currentContext.Value = null;
        }

        private IWebDriver CreateDriver(string browserName, bool headless)
        {
            browserName = browserName.ToLowerInvariant().Trim();
            _currentBrowser.Value = browserName;

            IWebDriver driver = browserName switch
            {
                "firefox" => CreateFirefox(headless),
                "edge" => CreateEdge(headless),
                _ => CreateChrome(headless)
            };

            Debug.Log($"{GetThreadLabel()} Driver iniciado (headless={headless})");
            return driver;
        }

        private IWebDriver CreateFirefox(bool headless)
        {
            var opts = new FirefoxOptions();
            if (headless) opts.AddArgument("--headless");
            return new FirefoxDriver(opts);
        }

        private IWebDriver CreateEdge(bool headless)
        {
            var opts = new EdgeOptions();
            opts.AddArgument("--edge-skip-compat-layer-relaunch");
            if (headless) opts.AddArgument("--headless=new");

            // Aislamiento del perfil
            string tempProfile = Path.Combine(Path.GetTempPath(), $"edge-profile-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempProfile);
            opts.AddArgument($"--user-data-dir={tempProfile}");

            return new EdgeDriver(opts);
        }

        private IWebDriver CreateChrome(bool headless)
        {
            var opts = new ChromeOptions();
            if (headless) opts.AddArgument("--headless=new");
            opts.AddArgument("--no-sandbox");
            opts.AddArgument("--disable-dev-shm-usage");
            opts.AddArgument("--disable-gpu");
            opts.AddArgument("--incognito");
            opts.AddArgument("--disable-extensions");
            opts.AddArgument("--remote-debugging-port=0");

            // Aislamiento del perfil
            string tempProfile = Path.Combine(Path.GetTempPath(), $"chrome-profile-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempProfile);
            opts.AddArgument($"--user-data-dir={tempProfile}");

            return new ChromeDriver(opts);
        }
        /// <summary>
        /// Devuelve la lista de drivers activos en el escenario actual.
        /// - En modo CI (matrix) o PARALLEL: devuelve un único driver.
        /// - En modo MULTI local: devuelve todos los navegadores creados.
        /// </summary>
        public static IReadOnlyList<IWebDriver> GetScenarioDrivers(ScenarioContext? context = null)
        {
            // Usa el contexto del hilo actual si no se pasa uno explícito
            var ctx = context ?? _currentContext.Value;

            if (ctx != null && ctx.TryGetValue("drivers", out var obj) && obj is List<IWebDriver> list && list.Count > 0)
                return list;

            // Fallback seguro: devuelve el driver actual si no hay lista
            return _currentDriver.Value != null
                ? new List<IWebDriver> { _currentDriver.Value }
                : Array.Empty<IWebDriver>();
        }

        public static string GetThreadLabel()
        {
            string browser = _currentBrowser.Value ?? "unknown";
            return $"[{browser}][Thread {Thread.CurrentThread.ManagedThreadId}]";
        }
    }
}
