using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SpecFlowLogin.Helpers.DebugTools;
using SpecFlowSelenium.Helpers;

namespace SpecFlowSelenium.Pages
{
    public class HomePage
    {
        private readonly IWebDriver _driver;
        private readonly IWait<IWebDriver> _wait;
        private readonly TestConfiguration _configuration;

        public HomePage(IWebDriver driver, IWait<IWebDriver> wait, TestConfiguration configuration)
        {
            _driver = driver;
            _wait = wait;
            _configuration = configuration;
        }

        /// <summary>
        /// Navega a la pantalla de Login y espera a que el formulario esté disponible.
        /// </summary>
        public HomePage Navigate()
        {
            var loginUri = new Uri(_configuration.BaseUrl, "login");
            Debug.Log($"Navegando a {loginUri}");

            _driver.Navigate().GoToUrl(loginUri);

            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("username")));
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new InvalidOperationException("La página de login no cargó dentro del tiempo configurado.", ex);
            }

            return this;
        }

        /// <summary>
        /// Inicia sesión con las credenciales proporcionadas.
        /// </summary>
        public HomePage Login(string user, string pass)
        {
            try
            {
                var userField = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("username")));
                userField.Clear();
                userField.SendKeys(user);

                var passField = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("password")));
                passField.Clear();
                passField.SendKeys(pass);

                var loginButton = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit']")));
                loginButton.Click();
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new InvalidOperationException("No fue posible completar el login porque el formulario no respondió a tiempo.", ex);
            }

            return this;
        }

        public bool IsDashboardVisible() => GetFlashMessage(By.CssSelector(".flash.success")) != null;

        public bool IsErrorVisible() => GetFlashMessage(By.CssSelector(".flash.error")) != null;

        public bool IsValidationVisible() => GetFlashMessage(By.CssSelector(".flash")) != null;

        private string? GetFlashMessage(By locator)
        {
            try
            {
                return _wait.Until(driver =>
                {
                    var element = driver.FindElements(locator).FirstOrDefault(e => e.Displayed);
                    if (element == null)
                    {
                        return null;
                    }

                    var text = element.Text?.Trim();
                    return string.IsNullOrWhiteSpace(text) ? null : text;
                });
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }
    }
}
