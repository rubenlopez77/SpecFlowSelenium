using OpenQA.Selenium;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Elemento proxy que replica las acciones sobre varios navegadores a la vez.
    /// </summary>
    public class MultiWebElement : IWebElement
    {
        private readonly IReadOnlyDictionary<string, IWebElement> _elements;

        public MultiWebElement(IReadOnlyDictionary<string, IWebElement> elements)
        {
            _elements = elements;
        }

        public bool Displayed => _elements.Values.First().Displayed;
        public bool Enabled => _elements.Values.First().Enabled;
        public Point Location => _elements.Values.First().Location;
        public bool Selected => _elements.Values.First().Selected;
        public Size Size => _elements.Values.First().Size;
        public string TagName => _elements.Values.First().TagName;
        public string Text => _elements.Values.First().Text;

        public void Clear() => Parallel.ForEach(_elements.Values, e => e.Clear());
        public void Click() => Parallel.ForEach(_elements.Values, e => e.Click());
        public IWebElement FindElement(By by) =>
            new MultiWebElement(_elements.ToDictionary(k => k.Key, v => v.Value.FindElement(by)));

        public ReadOnlyCollection<IWebElement> FindElements(By by) =>
            _elements.Values.First().FindElements(by);

        public string GetAttribute(string attributeName) => _elements.Values.First().GetAttribute(attributeName);
        public string GetCssValue(string propertyName) => _elements.Values.First().GetCssValue(propertyName);
        public string GetDomAttribute(string attributeName) => _elements.Values.First().GetDomAttribute(attributeName);
        public string GetDomProperty(string propertyName) => _elements.Values.First().GetDomProperty(propertyName);
        public ISearchContext GetShadowRoot() => _elements.Values.First().GetShadowRoot();
        public void SendKeys(string text) => Parallel.ForEach(_elements.Values, e => e.SendKeys(text));
        public void Submit() => Parallel.ForEach(_elements.Values, e => e.Submit());
    }
}
