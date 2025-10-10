using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SpecFlowLogin.Helpers.DebugTools;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Helpers
{
    [Binding]
    public class DriverHooks
    {
        private readonly IObjectContainer _container;
        private readonly TestConfiguration _configuration;
        private readonly List<BrowserSession> _activeSessions = new();

        public DriverHooks(IObjectContainer container, TestConfiguration configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        [BeforeScenario(Order = 0)]
        public async Task CreateScenarioDrivers()
        {
            var browsers = _configuration.Browsers;
            var timeout = _configuration.DefaultTimeout;

            Debug.Log($"Creando drivers para: {string.Join(", ", browsers)} (headless={_configuration.Headless})...");

            var sessionTasks = browsers.Select(browser => Task.Run(() =>
            {
                try
                {
                    Debug.Log($"[{browser}] Inicializando driver...");
                    var driver = CreateDriver(browser, _configuration.Headless);
                    var wait = new WebDriverWait(new SystemClock(), driver, timeout, TimeSpan.FromMilliseconds(500));
                    Debug.Log($"[{browser}] Driver listo.");
                    return new BrowserSession(browser, driver, wait);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"No fue posible iniciar el navegador '{browser}'.", ex);
                }
            })).ToArray();

            var sessions = await Task.WhenAll(sessionTasks);

            if (sessions.Length == 0)
            {
                throw new InvalidOperationException("No se pudo inicializar ningún driver de navegador para el escenario.");
            }

            foreach (var session in sessions)
            {
                _activeSessions.Add(session);
            }

            var readOnlySessions = new ReadOnlyCollection<BrowserSession>(sessions);
            _container.RegisterInstanceAs<IReadOnlyCollection<BrowserSession>>(readOnlySessions);
            _container.RegisterInstanceAs<IEnumerable<BrowserSession>>(readOnlySessions);

            // Compatibilidad con steps que dependan de IWebDriver/IWait concretos.
            var primarySession = sessions.First();
            _container.RegisterInstanceAs(primarySession.Driver);
            _container.RegisterInstanceAs(primarySession.Wait);
        }

        [AfterScenario(Order = 100)]
        public void CleanupDrivers()
        {
            foreach (var session in _activeSessions)
            {
                try
                {
                    Debug.Log($"[{session.Name}] Cerrando driver...");
                    session.Driver.Quit();
                }
                catch (Exception ex)
                {
                    Debug.Log($"[{session.Name}] Error al cerrar el driver: {ex.Message}");
                }
                finally
                {
                    session.Driver.Dispose();
                }
            }

            _activeSessions.Clear();
        }

        private IWebDriver CreateDriver(string browserName, bool headless)
        {
            browserName = browserName.ToLowerInvariant();

            try
            {
                return browserName switch
                {
                    "firefox" => BuildFirefox(headless),
                    "edge" => BuildEdge(headless),
                    _ => BuildChrome(headless)
                };
            }
            catch (DriverServiceNotFoundException ex)
            {
                throw new InvalidOperationException($"No se encontró el driver necesario para '{browserName}'.", ex);
            }
        }

        private static IWebDriver BuildChrome(bool headless)
        {
            var options = new ChromeOptions();
            if (headless)
            {
                options.AddArgument("--headless=new");
            }

            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1280,800");

            return new ChromeDriver(options);
        }

        private static IWebDriver BuildFirefox(bool headless)
        {
            var options = new FirefoxOptions();
            if (headless)
            {
                options.AddArgument("-headless");
            }

            return new FirefoxDriver(options);
        }

        private static IWebDriver BuildEdge(bool headless)
        {
            var options = new EdgeOptions();
            if (headless)
            {
                options.AddArgument("headless");
            }

            return new EdgeDriver(options);
        }
    }
}
