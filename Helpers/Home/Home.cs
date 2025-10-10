using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SpecFlowSelenium.Helpers;

namespace SpecFlowSelenium.Pages
{
    public class HomePage
    {
       

        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly TestConfiguration _config;

        public HomePage(DriverContext context)
        {
            _driver = context.Driver;
            _wait = context.Wait;
            _config = context.Config;
        }

        /// <summary>
        /// Navega a la pantalla de Login.
        /// </summary>
        /// <returns></returns>
        public HomePage Navigate()
        {
            _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("BASE_URL") + "/login");
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
            _driver.FindElement(By.Id("username")).SendKeys(user);
            _driver.FindElement(By.Id("password")).SendKeys(pass);
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            return this;
        }

        public bool IsDashboardVisible()
        {
            return _driver.FindElement(By.CssSelector(".flash.success")).Displayed;
        }

        public bool IsErrorVisible()
        {
            return _driver.FindElement(By.CssSelector(".flash.error")).Displayed;
        }

        public bool IsValidationVisible()
        {
            return _driver.FindElement(By.CssSelector(".flash")).Displayed;
        }
    }
}
