using OpenQA.Selenium;
using SpecFlowSelenium.Helpers;

namespace SpecFlowSelenium.Pages
{
    public class HomePage
    {
        private IWebDriver Driver => DriverFactory.CurrentDriver;

        public HomePage() { }

        /// <summary>
        /// Navega a la pantalla de Login.
        /// </summary>
        /// <returns></returns>
        public HomePage Navigate()
        {
            Driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("BASE_URL") + "/login");
            return this;
        }

        /// <summary>
        /// Inicia sesión con las credenciales proporcionadas.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public HomePage Login(string user, string pass)
        {
            Driver.FindElement(By.Id("username")).SendKeys(user);
            Driver.FindElement(By.Id("password")).SendKeys(pass);
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            return this;
        }

        public bool IsDashboardVisible()
        {
            return Driver.FindElement(By.CssSelector(".flash.success")).Displayed;
        }

        public bool IsErrorVisible()
        {
            return Driver.FindElement(By.CssSelector(".flash.error")).Displayed;
        }

        public bool IsValidationVisible()
        {
            return Driver.FindElement(By.CssSelector(".flash")).Displayed;
        }
    }
}
