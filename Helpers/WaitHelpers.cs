using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Utilidades para esperas explícitas con Selenium.
    /// Usa el driver actual (<see cref="DriverFactory.CurrentDriver"/>) aislado por ThreadLocal.
    /// Permite esperar por visibilidad, desaparición o texto de elementos.
    /// </summary>
    public static class WaitHelpers
    {
        private static IWebDriver Driver => DriverFactory.CurrentDriver;

        private static WebDriverWait Wait =>
            new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        /// <summary>
        /// Espera hasta que un elemento localizado por el selector <paramref name="by"/> sea visible.
        /// </summary>
        public static void WaitElementVisible(By by)
        {
            Wait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(by);
                    return el.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Espera hasta que un elemento localizado por <paramref name="by"/> desaparezca o deje de ser visible.
        /// </summary>
        public static void WaitElementDisappear(By by)
        {
            Wait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(by);
                    return !el.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return true;
                }
            });
        }


        /// <summary>
        /// Espera hasta que un elemento contenga el texto indicado.
        /// </summary>
        public static void WaitTextInElement(By by, string expectedText)
        {
            Wait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(by);
                    return el.Displayed && el.Text.Contains(expectedText, StringComparison.OrdinalIgnoreCase);
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });


        }
    }
}
