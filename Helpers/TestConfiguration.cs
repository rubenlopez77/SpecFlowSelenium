namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Configuración general de las pruebas.
    /// Para poder cargar credenciales de admin o de ciertos usuarios con permisos
    /// </summary>
    public class TestConfiguration
    {
        public Credentials ValidCredentials { get; }

        public TestConfiguration()
        {
            var user = Environment.GetEnvironmentVariable("USER") ?? "user";
            var pass = Environment.GetEnvironmentVariable("PASS") ?? "pass";

            ValidCredentials = new Credentials(user, pass);
        }
    }

 
    public class Credentials
    {
        public string Username { get; }
        public string Password { get; }

        public Credentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
