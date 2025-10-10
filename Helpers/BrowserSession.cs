using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Representa una sesión de navegador levantada para un escenario concreto.
    /// </summary>
    public sealed class BrowserSession
    {
        public BrowserSession(string name, IWebDriver driver, IWait<IWebDriver> wait)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = wait ?? throw new ArgumentNullException(nameof(wait));
        }

        /// <summary>
        /// Nombre legible del navegador (chrome, firefox, edge, ...).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Instancia de WebDriver asociada a la sesión.
        /// </summary>
        public IWebDriver Driver { get; }

        /// <summary>
        /// Espera explícita reutilizable para la sesión.
        /// </summary>
        public IWait<IWebDriver> Wait { get; }
    }
}
