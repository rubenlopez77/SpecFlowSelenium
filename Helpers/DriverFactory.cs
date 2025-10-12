using TechTalk.SpecFlow;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using DotNetEnv;
using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools;

namespace SpecFlowSelenium.Helpers
{
    [Binding, Parallelizable(ParallelScope.Fixtures)]
    public class DriverFactory
    {
        private static readonly ThreadLocal<IWebDriver> _currentDriver = new();
        private static readonly ThreadLocal<string> _currentBrowser = new();
        private static readonly ThreadLocal<ScenarioContext?> _currentContext = new();

        public static IWebDriver CurrentDriver
        {
            get
            {
                if (_currentDriver.Value == null)
                    throw new InvalidOperationException("ERROR: CurrentDriver no inicializado. Se anulan las pruebas");
                return _currentDriver.Value;
            }
            private set => _currentDriver.Value = value ?? throw new ArgumentNullException(nameof(value), "ERROR: CurrentDriver no está asignado.");
        }

        public DriverFactory(ScenarioContext context)
        {
            _currentContext.Value = context; // Guardamos el contexto del escenario actual en este hilo
        }

        [BeforeTestRun(Order = 0)]
        public static void LoadEnv()
        {
            try { Env.Load(); }
            catch { Console.WriteLine("ERROR: No se encontró .env"); }
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            _currentBrowser.Value = "setup";

            var context = _currentContext.Value;
            if (context == null)
                throw new InvalidOperationException("ScenarioContext no disponible en BeforeScenario");

            var browsersEnv = Environment.GetEnvironmentVariable("BROWSERS") ?? "chrome";
            var allBrowsers = browsersEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .ToArray();

            Debug.Log($"BROWSERS detectados: {string.Join(", ", allBrowsers)}");

            bool headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "true")
                .Trim().ToLowerInvariant() == "true";

            var execMode = (Environment.GetEnvironmentVariable("EXECUTION_MODE") ?? "PARALLEL")
                .Trim().ToUpperInvariant();

            Debug.Log($"Escenario: '{context.ScenarioInfo.Title}'");
            Debug.Log($"EXECUTION_MODE={execMode}");

            var drivers = new List<IWebDriver>();

            if (execMode == "MULTI")
            {
                Debug.Log($"Lanzando escenario en múltiples navegadores: {string.Join(", ", allBrowsers)}");

                foreach (var browser in allBrowsers)
                {
                    var driver = CreateDriver(browser, headless);
                    drivers.Add(driver);
                }
            }
            else
            {
                var browser = allBrowsers.First();
                Debug.Log($"Lanzando escenario en un único navegador: {browser}");
                var driver = CreateDriver(browser, headless);
                drivers.Add(driver);
            }

            context["drivers"] = drivers;
            CurrentDriver = drivers.First();
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
                        Debug.Log("Cerrando navegador...");
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

            Debug.Log($"Driver iniciado (headless={headless})");
            return driver;
        }

        private IWebDriver CreateFirefox(bool headless)
        {
            var fopts = new FirefoxOptions();
            if (headless) fopts.AddArgument("--headless");
            return new FirefoxDriver(fopts);
        }

        private IWebDriver CreateEdge(bool headless)
        {
            var eopts = new EdgeOptions();
            if (headless) eopts.AddArgument("headless");
            return new EdgeDriver(eopts);
        }

        private IWebDriver CreateChrome(bool headless)
        {
            var copts = new ChromeOptions();
            if (headless) copts.AddArgument("--headless=new");
            copts.AddArgument("--disable-gpu");
            copts.AddArgument("--no-sandbox");
            copts.AddArgument("--disable-dev-shm-usage");
            return new ChromeDriver(copts);
        }

        public static IReadOnlyList<IWebDriver> GetScenarioDrivers(ScenarioContext? context = null)
        {
            var ctx = context ?? _currentContext.Value;
            if (ctx != null && ctx.TryGetValue("drivers", out var obj) && obj is List<IWebDriver> list && list.Count > 0)
                return list;

            return new List<IWebDriver> { CurrentDriver };
        }

        public static string GetThreadLabel()
        {
            string browser = _currentBrowser.Value ?? "unknown";
            return $"[{browser}][Thread {Thread.CurrentThread.ManagedThreadId}]";
        }
    }
}
