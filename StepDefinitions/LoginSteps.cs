using TechTalk.SpecFlow;
using SpecFlowSelenium.Pages;
using static NUnit.Framework.Assert;
using SpecFlowSelenium.Helpers;

namespace SpecFlowSelenium.Steps
{
    [Binding]
    public class LoginSteps
    {

        /// <summary>
        ///
        /// Se utilizan helpers (como HomePage) para centralizar la interacción con los navegadores,
        /// lo que permite encadenar métodos.
        ///
        /// Esto facilita la escritura/lectura de pruebas claras, evita duplicación de código y mejora el mantenimiento.
        ///
        /// Paralelización y multi-navegador:
        /// - Cada escenario puede ejecutarse en múltiples navegadores definidos en el .env.
        /// - Los helpers utilizan DriverFactory.CurrentDriver para acceder al driver actual de cada thread.
        /// - Gracias a ThreadLocal<IWebDriver>, cada thread obtiene su propia instancia de navegador sin interferencias.
        ///
        /// - Todos los métodos de los helpers son síncronos (Selenium), por lo que no se requiere 'await'.
        /// - La arquitectura permite extenderse con otros helpers (por ejemplo DashboardPage, SettingsPage)
        ///   sin exponer la lista de drivers en los Steps.
        /// </summary>

        private readonly HomePage _home = new HomePage();
        private readonly TestConfiguration _config = new();


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
           //TODO obsoleto 
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
