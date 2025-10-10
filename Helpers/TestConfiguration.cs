using System;
using System.Collections.Generic;
using System.IO;
using DotNetEnv;

namespace SpecFlowSelenium.Helpers
{
    /// <summary>
    /// Configuración general de las pruebas.
    /// Se encarga de cargar las variables de entorno necesarias y validar que estén presentes
    /// antes de iniciar la ejecución. Cualquier ausencia provoca un fail-fast para evitar
    /// escenarios parcialmente configurados.
    /// </summary>
    public class TestConfiguration
    {
        private readonly Settings _settings;

        public TestConfiguration()
        {
            _settings = LoadSettings();
        }

        public Credentials ValidCredentials => _settings.Credentials;

        public Uri BaseUrl => _settings.BaseUrl;

        public IReadOnlyList<string> Browsers => _settings.Browsers;

        public bool Headless => _settings.Headless;

        public TimeSpan DefaultTimeout => _settings.DefaultTimeout;

        private static Settings LoadSettings()
        {
            try
            {
                Env.TraversePath().Load();
            }
            catch (FileNotFoundException ex)
            {
                throw new InvalidOperationException("No se pudo cargar el archivo .env requerido para las pruebas.", ex);
            }

            var baseUrl = GetRequired("BASE_URL");
            var username = GetRequired("USER");
            var password = GetRequired("PASS");

            var browsers = ParseBrowsers(Environment.GetEnvironmentVariable("BROWSERS"));
            var headless = ParseBoolean(Environment.GetEnvironmentVariable("HEADLESS"), defaultValue: true);
            var timeout = ParseTimeout(Environment.GetEnvironmentVariable("TIMEOUT_SECONDS"));

            return new Settings(
                new Uri(baseUrl, UriKind.Absolute),
                new Credentials(username, password),
                browsers,
                headless,
                timeout
            );
        }

        private static IReadOnlyList<string> ParseBrowsers(string? rawValue)
        {
            var values = string.IsNullOrWhiteSpace(rawValue)
                ? new[] { "chrome" }
                : rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (values.Length == 0)
            {
                throw new InvalidOperationException("La variable BROWSERS debe contener al menos un navegador válido.");
            }

            return values;
        }

        private static bool ParseBoolean(string? rawValue, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return defaultValue;
            }

            if (bool.TryParse(rawValue, out var result))
            {
                return result;
            }

            throw new InvalidOperationException("La variable HEADLESS debe ser true o false.");
        }

        private static TimeSpan ParseTimeout(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return TimeSpan.FromSeconds(10);
            }

            if (int.TryParse(rawValue, out var seconds) && seconds > 0)
            {
                return TimeSpan.FromSeconds(seconds);
            }

            throw new InvalidOperationException("La variable TIMEOUT_SECONDS debe ser un entero positivo.");
        }

        private static string GetRequired(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            throw new InvalidOperationException($"La variable de entorno '{key}' es obligatoria.");
        }

        private sealed record Settings(Uri BaseUrl, Credentials Credentials, IReadOnlyList<string> Browsers, bool Headless, TimeSpan DefaultTimeout);
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
