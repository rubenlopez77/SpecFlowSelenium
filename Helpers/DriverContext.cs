using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace SpecFlowSelenium.Helpers
{
    public class DriverContext
    {
        public IWebDriver Driver { get; }
        public WebDriverWait Wait { get; }
        public TestConfiguration Config { get; }

        public DriverContext(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            Config = new TestConfiguration();
        }
    }
}
