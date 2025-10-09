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
    [Binding, Parallelizable(ParallelScope.All)]
    public class DriverFactory
    {
        private static ThreadLocal<IWebDriver> _currentDriver = new();

        public static IWebDriver CurrentDriver
        {
            get
            {
                if (_currentDriver.Value == null)
                    throw new InvalidOperationException(
                        "ERROR: CurrentDriver no inicializado. Se anulan las pruebas"
                    );
                return _currentDriver.Value;
            }
            set => _currentDriver.Value = value ?? throw new ArgumentNullException(nameof(value), "ERROR: CurrentDriver no está asignado.");
        }
        private readonly ScenarioContext _context;
        
        
        public DriverFactory(ScenarioContext context) => _context = context;

        [BeforeTestRun(Order = 0)]
        public static void LoadEnv()
        {
            try { Env.Load(); } catch { Console.WriteLine("ERROR: No se encontró .env"); }
        }

        [BeforeScenario(Order = 0)]
        public async Task BeforeScenario()
        {
            
            var browsersEnv = Environment.GetEnvironmentVariable("BROWSERS");
            var allBrowsers = string.IsNullOrWhiteSpace(browsersEnv)
                ? new[] { "chrome" }
                : browsersEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // lanzar todos navegadores o solo 1 según tags
            bool fullParallel = _context.ScenarioInfo.Tags.Contains("smoke");
            var browsers = fullParallel ? allBrowsers : new[] { allBrowsers.First() };

            Debug.Log($"Escenario '{_context.ScenarioInfo.Title}' en: { string.Join(", ", browsers)}");
        
            bool headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "true").Trim().ToLowerInvariant() == "true";

            Debug.Log($"Creamos los Drivers en paralelo...");

            var tasks = browsers.Select(browser => Task.Run(() =>
            {
                Debug.Log($"Lanzando '{browser}' (headless={headless})...");
                var driver = CreateDriver(browser, headless);

                Debug.Log($"[Thread {Thread.CurrentThread.ManagedThreadId}] {browser} OK!");

                return driver;
            })).ToArray();

            var drivers = await Task.WhenAll(tasks);
            _context["drivers"] = drivers.ToList();

            // CurrentDriver a cada hilo. Importante
            CurrentDriver = drivers.First();
        }

        [AfterScenario(Order = 0)]
        public async Task AfterScenario()
        {
            if (_context.TryGetValue("drivers", out List<IWebDriver> drivers))
            {
                var closeTasks = drivers.Select(d => Task.Run(() =>
                {
                    try
                    {
                        Debug.Log($"Cerrando ...");
                        d.Quit();
                        d.Dispose();
                        Debug.Log($"Cerrado OK!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"Error cerrando: {ex.Message}");
                    }
                })).ToArray();

                await Task.WhenAll(closeTasks);
            }
        }
        private IWebDriver CreateDriver(string browserName, bool headless)
        {
            browserName = browserName.ToLowerInvariant().Trim();

            switch (browserName)
            {

                // TODO. Mas implementaciones segun Browser
                case "firefox":
                    var fopts = new FirefoxOptions();
                    if (headless) fopts.AddArgument("--headless");
                    return new FirefoxDriver(fopts);

                case "edge":
                    var eopts = new EdgeOptions();
                    if (headless) eopts.AddArgument("headless");
                    return new EdgeDriver(eopts);

                case "chrome":
                default:
                    var copts = new ChromeOptions();
                    if (headless) copts.AddArgument("--headless=new");
                    copts.AddArgument("--disable-gpu");
                    return new ChromeDriver(copts);
            }
        }
    }
}
