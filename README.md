# SpecFlow Multi-Browser Parallel Test Suite (Experimental)

![Parallel Scenarios](https://github.com/rubenlopez77/SpecFlowSelenium/actions/workflows/ci.yml/badge.svg)

**⚠️ Prueba de concepto (PoC)** — Este proyecto se utiliza únicamente para **experimentar con ejecución paralela y multi-navegador** usando **SpecFlow + Selenium**.  
Su propósito es probar la robustez, el aislamiento de contextos y la eficiencia en pipelines CI/CD. No está destinado a entornos productivos.

Current version: **v0.0.0**


---

## 🚀 Objetivo

Demostrar cómo implementar pruebas automatizadas BDD (Behavior Driven Development) con:

- Escenarios escritos en Gherkin (`.feature`)
- Definiciones de pasos en C#
- Ejecución **en paralelo** (ParallelScope.All)
- Pruebas **cross-browser simultáneas** con `MultiDriver`
- Integración con Selenium WebDriver para interacción real con el navegador
- Helpers reutilizables (Page Objects) para reducir duplicación y mejorar mantenimiento
- Estrategia de tags: diferenciar *Smoke* y *Regresión* (velocidad vs cobertura)

---

## 🧰 Tecnologías

- [.NET 8.0](https://dotnet.microsoft.com/)
- [SpecFlow](https://specflow.org/)
- [Selenium WebDriver](https://www.selenium.dev/)
- [NUnit](https://nunit.org/)
- GitHub Actions (CI/CD con matriz dual-mode)
- DotNetEnv (para configuración con `.env`)

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

El proyecto incluye un **pipeline dual-mode para GitHub Actions**:  
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

## 🔧 Último cambio: aislamiento de perfiles temporales

Para evitar conflictos entre navegadores (especialmente Edge y Chrome en Linux CI),  
cada navegador ahora usa su propio directorio temporal bajo `/tmp/wd-profiles/{browser}/profile-{GUID}`.

Ejemplo de implementación:

```csharp
string baseDir = Path.Combine(Path.GetTempPath(), "wd-profiles", browserName);
Directory.CreateDirectory(baseDir);
string profile = Path.Combine(baseDir, $"profile-{Guid.NewGuid()}");
```

Esto garantiza que cada instancia sea completamente independiente, incluso en ejecución paralela o en entornos CI.

---

## 📈 Próximos pasos

- [ ] Implementar patrón Page Object completo  
- [ ] Mejorar sistema de logging (NLog / Serilog)  
- [ ] Generar reportes visuales con SpecFlow+ LivingDoc  
- [ ] Añadir pruebas de regresión y smoke tags diferenciados  
- [ ] Integrar métricas de rendimiento (tiempos por Step / Escenario)

---

🧠 **Nota:**  
Este proyecto es una **prueba de concepto experimental** centrada en el aprendizaje y la evaluación de entornos paralelos con Selenium y SpecFlow.


**Nota:**  
El modo `MULTI` es **experimental** consume más recursos, ya que ejecuta múltiples navegadores en espejo.  
Usar preferentemente `PARALLEL` para regresiones continuas y reservar `MULTI` para validación cross-browser o compatibilidad visual.


🧰 Troubleshooting
⚠️ Edge browser issues on GitHub Actions (Ubuntu runners)

When running cross-browser SpecFlow + Selenium tests in GitHub Actions using the
ubuntu-latest runner, you may encounter errors when launching Microsoft Edge.