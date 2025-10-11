using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Implementa INavigation replicando acciones en múltiples drivers en paralelo.
    /// Compatible con versiones de Selenium que exponen métodos sync y async.
    /// </summary>
    public class MultiNavigation : INavigation
    {
        private readonly IReadOnlyDictionary<string, IWebDriver> _drivers;

        public MultiNavigation(IReadOnlyDictionary<string, IWebDriver> drivers)
        {
            _drivers = drivers;
        }

        // ----------- SÍNCRONOS -----------
        public void Back() => Parallel.ForEach(_drivers.Values, d => d.Navigate().Back());
        public void Forward() => Parallel.ForEach(_drivers.Values, d => d.Navigate().Forward());
        public void Refresh() => Parallel.ForEach(_drivers.Values, d => d.Navigate().Refresh());

        public void GoToUrl(string url) =>
            Parallel.ForEach(_drivers.Values, d => d.Navigate().GoToUrl(url));

        public void GoToUrl(Uri url) =>
            Parallel.ForEach(_drivers.Values, d => d.Navigate().GoToUrl(url));

        // ----------- ASÍNCRONOS (dos variantes: con y sin CancellationToken) -----------

        // Sin token (algunas versiones de Selenium definen exactamente estas firmas)
        public Task BackAsync() => Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().Back())));
        public Task ForwardAsync() => Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().Forward())));
        public Task RefreshAsync() => Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().Refresh())));
        public Task GoToUrlAsync(string url) => Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().GoToUrl(url))));
        public Task GoToUrlAsync(Uri url) => Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().GoToUrl(url))));

        // Con token (otras versiones añaden el token; tener ambos evita incompatibilidades)
        public Task BackAsync(CancellationToken cancellationToken) =>
            Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().Back(), cancellationToken)));

        public Task ForwardAsync(CancellationToken cancellationToken) =>
            Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().Forward(), cancellationToken)));

        public Task RefreshAsync(CancellationToken cancellationToken) =>
            Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().Refresh(), cancellationToken)));

        public Task GoToUrlAsync(string url, CancellationToken cancellationToken) =>
            Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().GoToUrl(url), cancellationToken)));

        public Task GoToUrlAsync(Uri url, CancellationToken cancellationToken) =>
            Task.WhenAll(_drivers.Values.Select(d => Task.Run(() => d.Navigate().GoToUrl(url), cancellationToken)));
    }
}
