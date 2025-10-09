# SpecFlow Multi-Browser Parallel Test Suite (Experimental)

**WIP!!** "Este proyecto es una suite de automatización **experimental** creada para explorar **SpecFlow con Selenium**, ejecutando pruebas en paralelo a través de múltiples navegadores.

---

## 🚀 Objetivo

Demostrar cómo implementar pruebas automatizadas BDD (Behavior Driven Development) con:
- Escenarios escritos en Gherkin (`.feature`)
- Run tests **in parallel** on Chrome, Firefox, and Edge. 
- Definiciones de pasos en C#
- Integración con Selenium WebDriver para interacción real con el navegador
- Helpers: Centraliza las interacciones con el navegador para reducir la duplicación de código. Simplifica el mantenimiento y lo hace escalable.
- Tagging Strategy: Flexibilidad al poder definir el tipo de pruebas al lanzar (Smoke y Regresión) Velocidad vs Cobertura en los pipelines de CI. Las pruebas Smoke por ejemplo sólo se lanzan en Chrome



---

## 🧰 Tecnologías

- [.NET 8.0](https://dotnet.microsoft.com/)
- [SpecFlow](https://specflow.org/)
- [Selenium WebDriver](https://www.selenium.dev/)
- [NUnit](https://nunit.org/) (o el framework de pruebas configurado)
- GitHub Actions (para CI/CD)

---

## 🗂️ Estructura del proyecto

```
SpecFlowSelenium/
├── Features/             # Escenarios en lenguaje Gherkin
├── StepDefinitions/      # Código C# que implementa los pasos
├── Helpers/              # Utilidades (DriverFactory, Configuración, etc.)
├── TestResults/          # Resultados de las ejecuciones
├── specflow.json         # Configuración de SpecFlow
├── .env                  # Variables de entorno (URLs, credenciales, etc.)
└── SetUpFixture.cs       # Configuración global de pruebas
```

---

## ⚙️ Configuración local

1. Instala el SDK de .NET 8.0 o superior.  
   ```bash
   dotnet --version
   ```
   Debe devolver una versión 8.0.x o superior.

2. Restaura las dependencias:
   ```bash
   dotnet restore
   ```

3. Crea el archivo `.env` (ya existe en el repo) con tus valores:
   ```ini
   BASE_URL=https://tusitio.com
   BROWSER=chrome
   HEADLESS=true
   ```

4. Ejecuta las pruebas:
   ```bash
   dotnet test
   ```

---

## 🧪 Ejemplo de escenario (Login.feature)

```gherkin
Feature: Login
  In order to access the application
  As a registered user
  I want to log in successfully

  Scenario: Successful login
    Given I am on the login page
    When I enter valid credentials
    Then I should be redirected to the home page
```

---

## 🔄 Integración continua (CI/CD)

Este proyecto incluye un workflow para **GitHub Actions** que ejecuta los tests automáticamente en cada `push` o `pull request`.

Archivo: `.github/workflows/dotnet-tests.yml`

Resultados de pruebas se almacenan como artefactos descargables en GitHub.

---

## 📈 Próximos pasos

- [ ] Implementar patrón Page Object  
- [ ] Mejora del sistema de LOG 
- [ ] Añadir logging estructurado (NLog / Serilog)  
- [ ] Generar reportes visuales con SpecFlow+ LivingDoc  
- [ ] Añadir más escenarios (registro, logout, búsqueda, etc.)
