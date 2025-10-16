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
        private readonly HomePage _home;
        private readonly TestConfiguration _config;
        /// <summary>
        /// La clase LoginSteps implementa los pasos del flujo de login dentro del marco BDD 
        /// utilizando un lenguaje natural y expresivo para describir la funcionalidad desde la perspectiva del usuario.
        /// Su objetivo principal es mantener la limpieza y legibilidad del código, 
        /// separando la lógica técnica de los steps.
        /// </summary>
        /// <param name="config"></param>

        public LoginSteps(TestConfiguration config)
        {
            _config = config;
            _home = new HomePage();
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
            _home.Login("usuario", "error");
        }

        [When("I leave credentials empty")]
        public void WhenILeaveCredentialsEmpty()
        {
            _home.Login("", "");
        }

        [When("I click the login button")]
        public void WhenIClickLoginButton()
        {
            // Obsoleto
        }

        [Then("I should see the dashboard")]
        public void ThenIShouldSeeTheDashboard()
        {
            That(_home.IsDashboardVisible());
        }

        [Then("I should see an error message")]
        public void ThenIShouldSeeAnErrorMessage()
        {
            That(_home.IsErrorVisible());
        }

        [Then("I should see a validation message")]
        public void ThenIShouldSeeAValidationMessage()
        {
            That(_home.IsValidationVisible());
        }
    }
}
