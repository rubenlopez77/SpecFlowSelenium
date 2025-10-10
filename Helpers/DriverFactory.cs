using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<IWebDriver> _activeDrivers = new();

        public DriverHooks(IObjectContainer container, TestConfiguration configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        [BeforeScenario(Order = 0)]
        public void CreateScenarioDriver()
        {
            var browser = _configuration.Browsers.First();
            Debug.Log($"Creando driver para '{browser}' (headless={_configuration.Headless})...");

            var driver = CreateDriver(browser, _configuration.Headless);
            _activeDrivers.Add(driver);

            _container.RegisterInstanceAs(driver);
            var wait = new WebDriverWait(new SystemClock(), driver, _configuration.DefaultTimeout, TimeSpan.FromMilliseconds(500));
            _container.RegisterInstanceAs<IWait<IWebDriver>>(wait);
        }

        [AfterScenario(Order = 100)]
        public void CleanupDrivers()
        {
            foreach (var driver in _activeDrivers)
            {
                try
                {
                    Debug.Log("Cerrando driver...");
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error al cerrar el driver: {ex.Message}");
                }
                finally
                {
                    driver.Dispose();
                }
            }

            _activeDrivers.Clear();
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
