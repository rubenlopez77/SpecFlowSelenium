using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SpecFlowSelenium.Helpers;
using SpecFlowSelenium.Pages;
using TechTalk.SpecFlow;
using static NUnit.Framework.Assert;

namespace SpecFlowSelenium.Steps
{
    [Binding]
    public class LoginSteps
    {
        private readonly IReadOnlyDictionary<string, HomePage> _homes;
        private readonly TestConfiguration _config;

        public LoginSteps(IReadOnlyCollection<BrowserSession> sessions, TestConfiguration config)
        {
            if (sessions == null || sessions.Count == 0)
            {
                throw new InvalidOperationException("No se creó ninguna sesión de navegador para el escenario actual.");
            }

            _config = config;
            _homes = sessions.ToDictionary(
                session => session.Name,
                session => new HomePage(session.Driver, session.Wait, config)
            );
        }

        [Given("I am on the login page")]
        public void GivenIAmOnLoginPage()
        {
            ExecuteOnAllBrowsers(home => home.Navigate());
        }

        [When("I enter valid credentials")]
        public void WhenIEnterValidCredentials()
        {
            ExecuteOnAllBrowsers(home => home.Login(_config.ValidCredentials.Username, _config.ValidCredentials.Password));
        }

        [When("I enter wrong password")]
        public void WhenIEnterWrongPassword()
        {
            ExecuteOnAllBrowsers(home => home.Login(_config.ValidCredentials.Username, "error"));
        }

        [When("I leave credentials empty")]
        public void WhenILeaveCredentialsEmpty()
        {
            ExecuteOnAllBrowsers(home => home.Login(string.Empty, string.Empty));
        }

        [Then("I should see the dashboard")]
        public void ThenIShouldSeeTheDashboard()
        {
            AssertOnAllBrowsers(home => home.IsDashboardVisible(), "Se esperaba el mensaje de éxito tras el login válido.");
        }

        [Then("I should see an error message")]
        public void ThenIShouldSeeAnErrorMessage()
        {
            AssertOnAllBrowsers(home => home.IsErrorVisible(), "Se esperaba un mensaje de error por contraseña inválida.");
        }

        [Then("I should see a validation message")]
        public void ThenIShouldSeeAValidationMessage()
        {
            AssertOnAllBrowsers(home => home.IsValidationVisible(), "Se esperaba un mensaje de validación por campos vacíos.");
        }

        private void ExecuteOnAllBrowsers(Action<HomePage> action)
        {
            Parallel.ForEach(_homes.Values, home => action(home));
        }

        private void AssertOnAllBrowsers(Func<HomePage, bool> assertion, string message)
        {
            var failures = new ConcurrentBag<string>();

            Parallel.ForEach(_homes, pair =>
            {
                if (!assertion(pair.Value))
                {
                    failures.Add(pair.Key);
                }
            });

            var failedBrowsers = failures.ToArray();
            That(failedBrowsers, Is.Empty, failedBrowsers.Length == 0
                ? string.Empty
                : $"{message} Navegadores con fallo: {string.Join(", ", failedBrowsers)}");
        }
    }
}
