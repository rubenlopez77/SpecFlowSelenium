using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Driver proxy que replica las acciones de WebDriver en varios navegadores simultáneamente.
    /// </summary>
    public class MultiDriver : IWebDriver
    {
        private readonly IReadOnlyDictionary<string, IWebDriver> _drivers;

        public MultiDriver(IReadOnlyDictionary<string, IWebDriver> drivers)
        {
            _drivers = drivers;
        }

        public string Url
        {
            get => _drivers.Values.First().Url;
            set => Parallel.ForEach(_drivers.Values, d => d.Url = value);
        }

        public string Title => _drivers.Values.First().Title;
        public string PageSource => _drivers.Values.First().PageSource;
        public string CurrentWindowHandle => _drivers.Values.First().CurrentWindowHandle;
        public ReadOnlyCollection<string> WindowHandles => _drivers.Values.First().WindowHandles;

        public void Close() => Parallel.ForEach(_drivers.Values, d => d.Close());
        public void Quit() => Parallel.ForEach(_drivers.Values, d => d.Quit());
        public void Dispose() => Quit();

        public IWebElement FindElement(By by)
        {
            var elements = _drivers.ToDictionary(k => k.Key, v => v.Value.FindElement(by));
            return new MultiWebElement(elements);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            // Solo devolvemos los del primero (proxy simplificado)
            return _drivers.Values.First().FindElements(by);
        }

        public INavigation Navigate() => new MultiNavigation(_drivers);

        public IOptions Manage() => _drivers.Values.First().Manage();
        public ITargetLocator SwitchTo() => _drivers.Values.First().SwitchTo();
    }
}
