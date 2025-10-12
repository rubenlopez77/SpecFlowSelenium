using OpenQA.Selenium;
using SpecFlowSelenium.Helpers;

namespace SpecFlowSelenium.Pages
{
    public class HomePage
    {
        private readonly List<IWebDriver> _drivers;

        public HomePage()
        {
         
            _drivers = DriverFactory.GetScenarioDrivers().ToList();
        }

        public HomePage Navigate()
        {
            var baseUrl = (Environment.GetEnvironmentVariable("BASE_URL") ?? "").TrimEnd('/');
            foreach (var d in _drivers)
                d.Navigate().GoToUrl($"{baseUrl}/login");
            return this;
        }

        public HomePage Login(string user, string pass)
        {
            foreach (var d in _drivers)
            {
                d.FindElement(By.Id("username")).SendKeys(user);
                d.FindElement(By.Id("password")).SendKeys(pass);
                d.FindElement(By.CssSelector("button[type='submit']")).Click();
            }
            return this;
        }

        public bool IsDashboardVisible()
            => _drivers.All(d => d.FindElement(By.CssSelector(".flash.success")).Displayed);

        public bool IsErrorVisible()
            => _drivers.All(d => d.FindElement(By.CssSelector(".flash.error")).Displayed);

        public bool IsValidationVisible()
            => _drivers.All(d => d.FindElement(By.CssSelector(".flash")).Displayed);
    }
}
