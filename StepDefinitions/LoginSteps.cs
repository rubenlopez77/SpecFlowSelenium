using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SpecFlowSelenium.Helpers;
using SpecFlowSelenium.Pages;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Steps
{
    [Binding]
    public class LoginSteps
    {
        private readonly HomePage _home;
        private readonly TestConfiguration _config;

        public LoginSteps(IWebDriver driver, IWait<IWebDriver> wait, TestConfiguration config)
        {
            _config = config;
            _home = new HomePage(driver, wait, config);
        }

        [Given("I am on the login page")]
        public void GivenIAmOnLoginPage()
        {
            _home.Navigate();
        }

        [When("I enter valid credentials")]
        public void WhenIEnterValidCredentials()
        {
            _home.Login(_config.ValidCredentials.Username, _config.ValidCredentials.Password);
        }

        [When("I enter wrong password")]
        public void WhenIEnterWrongPassword()
        {
            _home.Login(_config.ValidCredentials.Username, "error");
        }

        [When("I leave credentials empty")]
        public void WhenILeaveCredentialsEmpty()
        {
            _home.Login(string.Empty, string.Empty);
        }

        [Then("I should see the dashboard")]
        public void ThenIShouldSeeTheDashboard()
        {
            Assert.That(_home.IsDashboardVisible(), Is.True, "Se esperaba el mensaje de éxito tras el login válido.");
        }

        [Then("I should see an error message")]
        public void ThenIShouldSeeAnErrorMessage()
        {
            Assert.That(_home.IsErrorVisible(), Is.True, "Se esperaba un mensaje de error por contraseña inválida.");
        }

        [Then("I should see a validation message")]
        public void ThenIShouldSeeAValidationMessage()
        {
            Assert.That(_home.IsValidationVisible(), Is.True, "Se esperaba un mensaje de validación por campos vacíos.");
        }
    }
}
