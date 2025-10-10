# SpecFlow Multi-Browser Parallel Test Suite (Experimental)

![Parallel Scenarios](https://github.com/rubenlopez77/SpecFlowSelenium/actions/workflows/ci.yml/badge.svg)

**WIP!!** Este proyecto es una suite de automatización **experimental** creada para explorar **SpecFlow con Selenium**, ejecutando pruebas en **paralelo** y en **múltiples navegadores**, con control de modo desde el archivo `.env`.

---

## 🚀 Objetivo

Demostrar cómo implementar pruebas automatizadas BDD (Behavior Driven Development) con:

- Escenarios escritos en Gherkin (`.feature`)
- Definiciones de pasos en C#
- Ejecución **en paralelo** (ParallelScope.All)
- Pruebas **cross-browser simultáneas** con `MultiDriver`
- Integración con Selenium WebDriver para interacción real con el navegador
- Helpers reutilizables (Page Objects) para reducir duplicación y mejorar el mantenimiento
- Estrategia de tags: diferenciar *Smoke* y *Regresión* (velocidad vs cobertura)

---

## 🧰 Tecnologías

- [.NET 8.0](https://dotnet.microsoft.com/)
- [SpecFlow](https://specflow.org/)
- [Selenium WebDriver](https://www.selenium.dev/)
- [NUnit](https://nunit.org/)
- GitHub Actions (CI/CD con matriz dual-mode)
- DotNetEnv (para configuración por `.env`)

---

## 🗂️ Estructura del proyecto

```
SpecFlowSelenium/
├── Features/             # Escenarios en lenguaje Gherkin
├── StepDefinitions/      # Código C# que implementa los pasos
├── Helpers/              # Utilidades (DriverFactory, MultiDriver, etc.)
├── TestResults/          # Resultados de las ejecuciones
├── specflow.json         # Configuración de SpecFlow
├── .env                  # Variables de entorno
└── .github/workflows/ci.yml  # Pipeline dual-mode (Parallel / Multi)
```

---

## ⚙️ Configuración local

1. Instala el SDK de .NET 8.0 o superior  
   ```bash
   dotnet --version
   ```

2. Restaura las dependencias:
   ```bash
   dotnet restore
   ```

3. Configura el archivo `.env`:
   ```ini
   BASE_URL=https://tusitio.com
   EXECUTION_MODE=PARALLEL     # o MULTI
   BROWSERS=chrome,firefox,edge
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

Este proyecto incluye un **pipeline dual-mode para GitHub Actions**:  
Archivo → `.github/workflows/ci.yml`

| Job | Trigger | Navegadores | Modo | Propósito |
|------|----------|--------------|--------|------------|
| 🧩 **Parallel Scenarios (Chrome)** | Automático en cada `push` o `PR` | Chrome | `EXECUTION_MODE=PARALLEL` | Validación rápida y ligera |
| 🌐 **Cross-Browser MultiDriver** | Manual desde Actions (`workflow_dispatch`) | Chrome, Firefox, Edge | `EXECUTION_MODE=MULTI` | Pruebas simultáneas cross-browser |

📦 Los resultados se almacenan como artefactos (`parallel-results` y `multi-results`).

---

## 🧩 Variables de entorno soportadas

| Variable | Descripción | Ejemplo |
|-----------|--------------|----------|
| `EXECUTION_MODE` | Define el modo de ejecución (`PARALLEL` o `MULTI`) | `EXECUTION_MODE=PARALLEL` |
| `BROWSERS` | Lista de navegadores separados por coma | `chrome,firefox,edge` |
| `HEADLESS` | Ejecuta los navegadores sin interfaz gráfica | `true` |
| `BASE_URL` | URL base de la aplicación bajo prueba | `https://demo.app` |

---

## 🩺 Troubleshooting

### ❗ Error: `session not created: probably user data directory is already in use`

**Causa:**  
Chrome o Edge están intentando compartir el mismo perfil de usuario (`--user-data-dir`) cuando se ejecutan varias pruebas en paralelo.

**Solución aplicada en el proyecto:**  
Cada hilo crea su propio perfil temporal único usando `Guid` y `ThreadId`:

```csharp
string profile = Path.Combine(Path.GetTempPath(), $"wd-profile-{browserName}-{Guid.NewGuid()}-{Thread.CurrentThread.ManagedThreadId}");
copts.AddArgument($"--user-data-dir={profile}");
```

Esto garantiza que cada instancia de navegador sea independiente, eliminando conflictos de sesión en modo paralelo.

---

## 📈 Próximos pasos

- [ ] Implementar patrón Page Object completo  
- [ ] Mejorar sistema de logging (NLog / Serilog)  
- [ ] Generar reportes visuales con SpecFlow+ LivingDoc  
- [ ] Añadir pruebas de regresión y smoke tags diferenciados  
- [ ] Integrar métricas de rendimiento (Tiempos por Step / Escenario)

---

**Nota:**  
El modo `MULTI` es **experimental** consume más recursos, ya que ejecuta múltiples navegadores en espejo.  
Usar preferentemente `PARALLEL` para regresiones continuas y reservar `MULTI` para validación cross-browser o compatibilidad visual.
