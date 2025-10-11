using BoDi;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using SpecFlowLogin.Helpers.DebugTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Gestiona el ciclo de vida de los navegadores en cada escenario SpecFlow.
    /// 
    ///   Características principales:
    /// - Lanza múltiples navegadores definidos en la variable de entorno BROWSERS (por ejemplo: chrome,firefox,edge).
    /// - Ejecuta las acciones en todos ellos en paralelo a través de <see cref="MultiDriver"/>.
    /// - Cada navegador usa un perfil temporal (--user-data-dir) aislado.
    /// - Se cierran todos los navegadores al final del escenario.
    /// 
    /// 🔹 Ejemplo de log:
    ///  Escenario 'Login' ejecutándose con: chrome, firefox, edge
    ///  Todos los navegadores cerrados correctamente.
    /// </summary>
    [Binding, Parallelizable(ParallelScope.All)]
    public class DriverFactory
    {
        private static ThreadLocal<IWebDriver> _currentDriver = new();

        public static IWebDriver CurrentDriver
        {
            get
            {
                if (_currentDriver.Value == null)
                    throw new InvalidOperationException("ERROR: CurrentDriver no inicializado.");
                return _currentDriver.Value;
            }
            set => _currentDriver.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private readonly ScenarioContext _context;
        private readonly IObjectContainer _container;

        // Mantiene todos los drivers del escenario
        private static ConcurrentDictionary<string, IWebDriver> _allDrivers = new();
        private readonly ConcurrentBag<string> _sandboxRoots = new();

        public DriverFactory(ScenarioContext context, IObjectContainer container)
        {
            _context = context;
            _container = container;
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            var mode = (Environment.GetEnvironmentVariable("EXECUTION_MODE") ?? "PARALLEL").Trim().ToUpperInvariant();
            var browsersEnv = Environment.GetEnvironmentVariable("BROWSERS") ?? "chrome";
            var browsers = browsersEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(b => b.ToLowerInvariant())
                .ToArray();

            bool headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "true")
                .Trim().ToLowerInvariant() == "true";

            Debug.Log($" [] Escenario '{_context.ScenarioInfo.Title}' con modo: {mode}");

            if (mode == "MULTI")
            {
                // ========== MODO MULTI (varios navegadores a la vez) ==========
                Debug.Log($" - Lanzando navegadores: {string.Join(", ", browsers)}");

                var drivers = new ConcurrentDictionary<string, IWebDriver>();

                Parallel.ForEach(browsers, browser =>
                {
                    try
                    {
                        var driver = CreateDriver(browser, headless);
                        drivers[browser] = driver;
                        Debug.Log($"- {browser} inicializado correctamente.");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"⚠️ Error lanzando {browser}: {ex.Message}");
                    }
                });

                var successfulDrivers = drivers
                    .Where(kvp => kvp.Value is not null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (successfulDrivers.Count == 0)
                {
                    Debug.Log("⚠️ No se pudo iniciar ningún navegador en modo MULTI. Se intentará un fallback a Chrome en modo individual.");

                    try
                    {
                        const string fallbackBrowser = "chrome";
                        var fallbackDriver = CreateDriver(fallbackBrowser, headless);

                        _context["drivers"] = new List<IWebDriver> { fallbackDriver };
                        _allDrivers = new ConcurrentDictionary<string, IWebDriver>(new[]
                        {
                            new KeyValuePair<string, IWebDriver>(fallbackBrowser, fallbackDriver)
                        });

                        CurrentDriver = fallbackDriver;
                        _container.RegisterInstanceAs(new DriverContext(fallbackDriver));

                        Debug.Log("Fallback a modo paralelo con Chrome completado.");
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            "No se pudo iniciar ningún navegador en modo MULTI ni completar el fallback a Chrome.",
                            ex
                        );
                    }

                    return;
                }

                var readOnlyDrivers = new ReadOnlyDictionary<string, IWebDriver>(successfulDrivers);

                _context["drivers"] = readOnlyDrivers.Values.ToList();
                _allDrivers = new ConcurrentDictionary<string, IWebDriver>(successfulDrivers);

                var multiDriver = new MultiDriver(readOnlyDrivers);
                CurrentDriver = multiDriver;
                _container.RegisterInstanceAs(new DriverContext(CurrentDriver));

                Debug.Log("MultiDriver activo (modo espejo cross-browser).");
            }
            else
            {
                // ========== MODO PARALLEL (1 navegador por escenario) ==========
                var browser = browsers.First();
                Debug.Log($"Paralelismo por escenario, navegador: {browser}");

                var driver = CreateDriver(browser, headless);
                _context["drivers"] = new List<IWebDriver> { driver };
                _allDrivers = new ConcurrentDictionary<string, IWebDriver>(new[] { new KeyValuePair<string, IWebDriver>(browser, driver) });

                CurrentDriver = driver;
                _container.RegisterInstanceAs(new DriverContext(driver));

                Debug.Log("Driver individual listo (modo paralelo).");
            }
        }

        [AfterScenario(Order = 0)]
        public void AfterScenario()
        {
            try
            {
                if (_context.TryGetValue("drivers", out List<IWebDriver> drivers))
                {
                    Debug.Log($" Cerrando {drivers.Count} navegadores...");

                    Parallel.ForEach(drivers, driver =>
                    {
                        try
                        {
                            driver.Quit();
                            driver.Dispose();
                            Debug.Log($"Navegador cerrado correctamente ({driver.GetType().Name})");
                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"Error cerrando {driver.GetType().Name}: {ex.Message}");
                        }
                    });
                }
                else
                {
                    Debug.Log("No se encontraron drivers en el contexto. Nada que cerrar.");
                }

                // Limpieza adicional (evita ArgumentNullException)
                _currentDriver.Value = null;
                _allDrivers.Clear();

                // Elimina los sandboxes temporales usados por los navegadores
                while (_sandboxRoots.TryTake(out var sandboxRoot))
                {
                    TryDeleteSandbox(sandboxRoot);
                }

                Debug.Log("Limpieza de drivers completada con éxito.");
            }
            catch (Exception ex)
            {
                Debug.Log($"Error en limpieza de navegadores: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea e inicializa un driver según el navegador indicado.
        /// Cada driver usa un perfil temporal para evitar conflictos entre hilos.
        /// </summary>
        private IWebDriver CreateDriver(string browserName, bool headless)
        {
            browserName = browserName.ToLowerInvariant().Trim();

            BrowserSandbox? sandbox = null;

            try
            {
                sandbox = CreateBrowserSandbox(browserName);

                IWebDriver driver = browserName switch
                {
                    "firefox" => CreateFirefoxDriver(headless, sandbox),
                    "edge" => CreateEdgeDriver(headless, sandbox),
                    "chrome" or _ => CreateChromeDriver(headless, sandbox)
                };

                _sandboxRoots.Add(sandbox.RootPath);

                return driver;
            }
            catch (Exception ex)
            {
                if (sandbox is not null)
                {
                    TryDeleteSandbox(sandbox.RootPath);
                }

                //FAIL FAST
                string msg = $"ERROR: No se pudo iniciar el navegador '{browserName}' — se aborta el escenario.\n{ex.Message}";
                Debug.Log(msg);
                throw new InvalidOperationException(msg, ex);
            }
        }

        private BrowserSandbox CreateBrowserSandbox(string browserName)
        {
            string uniqueSegment = string.Join(
                "_",
                DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                Environment.ProcessId,
                Environment.CurrentManagedThreadId,
                Guid.NewGuid().ToString("N")
            );

            string rootPath = Path.Combine(Path.GetTempPath(), "wd-sandboxes", browserName, uniqueSegment);
            string profilePath = Path.Combine(rootPath, "profile");
            string driverArtifactsPath = Path.Combine(rootPath, "driver-service");

            Directory.CreateDirectory(profilePath);
            Directory.CreateDirectory(driverArtifactsPath);

            return new BrowserSandbox(rootPath, profilePath, driverArtifactsPath);
        }

        private IWebDriver CreateChromeDriver(bool headless, BrowserSandbox sandbox)
        {
            var options = new ChromeOptions();
            options.AddArgument($"--user-data-dir={sandbox.ProfilePath}");
            options.AddArgument($"--profile-directory=Profile-{Guid.NewGuid():N}");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--remote-debugging-port=0");
            options.AddArgument("--no-first-run");
            options.AddArgument("--no-default-browser-check");
            if (headless)
            {
                options.AddArgument("--headless=new");
            }

            var service = ChromeDriverService.CreateDefaultService();
            service.Port = 0;
            service.EnableAppendLog = true;
            service.LogPath = Path.Combine(sandbox.DriverArtifactsPath, $"chromedriver-{Guid.NewGuid():N}.log");

            return new ChromeDriver(service, options);
        }

        private IWebDriver CreateEdgeDriver(bool headless, BrowserSandbox sandbox)
        {
            var options = new EdgeOptions();
            options.AddArgument($"--user-data-dir={sandbox.ProfilePath}");
            options.AddArgument($"--profile-directory=Profile-{Guid.NewGuid():N}");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--remote-debugging-port=0");
            if (headless)
            {
                options.AddArgument("--headless=new");
            }

            var service = EdgeDriverService.CreateDefaultService();
            service.Port = 0;
            service.LogPath = Path.Combine(sandbox.DriverArtifactsPath, $"edgedriver-{Guid.NewGuid():N}.log");

            return new EdgeDriver(service, options);
        }

        private IWebDriver CreateFirefoxDriver(bool headless, BrowserSandbox sandbox)
        {
            var options = new FirefoxOptions();
            if (headless)
            {
                options.AddArgument("--headless");
            }
            options.AddArguments("-profile", sandbox.ProfilePath);

            var service = FirefoxDriverService.CreateDefaultService();
            service.Host = "127.0.0.1";
            service.Port = 0;
            service.LogFile = Path.Combine(sandbox.DriverArtifactsPath, $"geckodriver-{Guid.NewGuid():N}.log");

            return new FirefoxDriver(service, options);
        }

        private static void TryDeleteSandbox(string rootPath)
        {
            try
            {
                if (Directory.Exists(rootPath))
                {
                    Directory.Delete(rootPath, true);
                }
            }
            catch (Exception cleanupEx)
            {
                Debug.Log($"No se pudo limpiar el sandbox temporal '{rootPath}': {cleanupEx.Message}");
            }
        }

        private sealed record BrowserSandbox(string RootPath, string ProfilePath, string DriverArtifactsPath);



        [AfterTestRun(Order = 999)]
        public static void AfterTestRun()
        {
            try
            {
                _currentDriver.Dispose();
                Debug.Log("ThreadLocal<IWebDriver> liberado correctamente tras todos los escenarios.");
            }
            catch (Exception ex)
            {
                Debug.Log($"Error liberando ThreadLocal: {ex.Message}");
            }
        }
    }
}
