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
        /// Clase correspondiente al flujo de pasos del login.
        ///
        ///   Arquitectura:
        /// - Este archivo define los pasos BDD (Given/When/Then) y delega toda la lógica de interacción con el navegador
        /// a los helpers o Page Objects (como <see cref="HomePage"/>).
        /// - Sigue el patrón Page Object Model (POM), que fomenta la reutilización de código, la legibilidad
        /// y el mantenimiento a largo plazo.
        ///
        ///   Estructura y flujo:
        /// - Cada Step invoca métodos de los Helpers (HomePage, DashboardPage, SettingsPage, etc.)
        /// que encapsulan las acciones sobre el DOM (clicks, inputs, asserts...).
        /// - Los helpers encadenan métodos para ofrecer una sintaxis fluida y expresiva en las pruebas.
        /// - Todos los métodos son síncronos (propios de Selenium), por lo que no requieren 'await'.
        ///
        ///   Paralelización y multi-navegador:
        /// - Los escenarios pueden ejecutarse en paralelo en distintos navegadores definidos en el archivo .env (por ejemplo, Chrome, Firefox, Edge).
        /// - <see cref="DriverFactory"/> gestiona internamente las instancias del navegador mediante <see cref="ThreadLocal{T}"/>.
        /// - Cada hilo de ejecución mantiene su propio IWebDriver aislado, evitando interferencias entre escenarios.
        /// - Los helpers obtienen el driver actual mediante <see cref="DriverFactory.CurrentDriver"/>, garantizando independencia por hilo.
        ///
        ///   Extensibilidad:
        /// - La arquitectura está pensada para escalar: se pueden agregar nuevos helpers (por ejemplo, DashboardPage o SettingsPage)
        /// sin modificar los Steps existentes.
        /// - Los Steps no acceden directamente a las listas de drivers ni a la configuración del entorno,
        /// manteniendo bajo acoplamiento y alta cohesión.
        ///
        ///   Beneficios:
        /// - Código más limpio y expresivo en los archivos .feature.
        /// - Mantenimiento simplificado al centralizar la lógica de interacción.
        /// - Ejecución paralela segura y compatible con SpecFlow + NUnit.
        /// </summary>
        /// 



        private readonly HomePage _home;
        private readonly TestConfiguration _config;

        public LoginSteps(DriverContext context, TestConfiguration config)
        {
            _home = new HomePage(context);
            _config = config;
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
