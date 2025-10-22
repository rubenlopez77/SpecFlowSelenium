# SpecFlow Selenium Tests


![Version Badge](https://img.shields.io/badge/version-v0.0.1-blue?style=for-the-badge)


[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=bugs)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)

# ⚠️ Automated UI Testing with SpecFlow + Selenium + .NET

**Prueba de concepto (PoC)** — Este proyecto es **experimental** y se utiliza únicamente para demostrar las capacidades de **SpecFlow + Selenium** en escenarios de ejecución **paralela** y **multi-navegador**.  
Su propósito es probar la **robustez**, el **aislamiento de contextos** y la eficacia de una **arquitectura de pruebas escalable**.  
No está destinado a entornos productivos.

---

## 🚀 Objetivo

Demostrar cómo implementar pruebas automatizadas **BDD (Behavior Driven Development)** con una arquitectura sólida y mantenible:

- ✅ **BDD:** Escenarios escritos en **Gherkin (.feature)**
- ✅ **Definiciones de pasos** en **C# (SpecFlow Steps)**
- ✅ Ejecución **en paralelo** con **NUnit** (`[ParallelScope.Fixtures]`)
- ✅ Pruebas **cross-browser simultáneas** con arquitectura **MultiDriver**
- ✅ Integración con **Selenium WebDriver** para interacción real con el navegador
- ✅ **Page Object Model (POM)** con **Fluent Interface** para lectura expresiva y encadenamiento fluido de acciones
- ✅ **Arquitectura por capas** (*Feature → Step Definitions → Page Objects → Core Utilities*) que **incrementa la cobertura** y **reduce los costes de mantenimiento**
- ✅ **Helpers y Page Objects** reutilizables para reducir duplicación
- ✅ **Estrategia de tags:** diferenciar **Smoke** y **Regresión** (velocidad vs cobertura)

------

## 🧩 Estructura del proyecto

```
SpecFlowSelenium/
│
├── Features/
│   └── Login.feature          # Escenarios en Gherkin
│
├── Steps/
│   └── LoginSteps.cs          # Definiciones Given/When/Then
│
├── Pages/
│   └── HomePage.cs            # Page Object principal
│
├── Helpers/
│   ├── DriverFactory.cs       # Control de drivers y paralelismo
│   ├── Debug.cs               # Logging multi-hilo con timestamps
│   └── WaitHelpers.cs         # Esperas explícitas
│
└── .github/workflows/
    ├── dual-mode-tests.yml    # Pipeline dual (Parallel + Multi)
    └── semantic-version.yml   # Versionado automático + badge README
```

## 📈 Beneficios clave

- Sintaxis legible y expresiva 
- Separación clara de responsabilidades
- Alta reutilización de componentes
- Mayor cobertura con menor mantenimiento
- Ejecución en paralelo y cross-browser
- Integración sencilla en pipelines **CI/CD**

  
------

## 🔧 Modos de ejecución (`EXECUTION_MODE`)

La variable de entorno `EXECUTION_MODE` controla cómo se ejecutan los navegadores en las pruebas.  
Existen tres modos disponibles:

| Modo | Descripción | Uso típico | Consumo de recursos | Riesgo de conflicto |
|------|--------------|------------|---------------------|--------------------|
| **PARALLEL** | Todos los navegadores a la vez (threads distintos) | Stress testing, validación simultánea, benchmarking | Alto | Mayor (acciones simultáneas, logs mezclados) |
| **MULTI** | Navegadores uno tras otro (mismo hilo) | Validación cruzada, depuración, CI estable | Medio | Casi nulo |
| **SINGLE (por omisión)** | Un solo navegador | Tests normales, desarrollo local | Bajo | Ninguno |


## 🧠 Arquitectura técnica

El sistema utiliza un modelo híbrido y escalable que soporta ejecución **multi-navegador** y **multihilo**, adaptándose automáticamente según el modo definido por `EXECUTION_MODE`.

| Elemento | Descripción |
|-----------|--------------|
| **DriverFactory** | Crea y gestiona instancias independientes de `IWebDriver` por navegador y escenario. Soporta ejecución **paralela real (multithread)** o **secuencial** según configuración. |
| **ThreadLocal** | Aísla los contextos de `IWebDriver`, `ScenarioContext` y metadatos del navegador por hilo, garantizando independencia total en `PARALLEL`. |
| **EXECUTION_MODE** | Controla el comportamiento de ejecución: `PARALLEL` (varios navegadores en hilos distintos), `MULTI` (uno tras otro en el mismo hilo) o `SINGLE` (un solo navegador por defecto). |
| **Page Objects** | Encapsulan la lógica de interacción con la interfaz de usuario, manteniendo el código de los steps limpio y reutilizable. |
| **ConcurrentBag / Parallel.ForEach** | Permiten crear y cerrar múltiples instancias de navegador simultáneamente, sin bloqueos ni condiciones de carrera. |
| **Debug.Log** | Emite trazas detalladas `[browser][Thread][HH:mm:ss]` en consola. El logger es thread-safe y facilita la depuración en escenarios concurrentes. |


---

## ⚙️ Variables de entorno `.env`

```bash
EXECUTION_MODE=PARALLEL    # o MULTI
BROWSERS=chrome,firefox,edge
HEADLESS=true
BASE_URL=https://example.com
LOG_TO_FILE=false
```

---

## 🧱 GitHub Actions — Dual Mode Pipeline

Este repositorio incluye un pipeline dual-mode configurado en  
**`.github/workflows/dual-mode-tests.yml`**:

| Job | Trigger | Navegadores | Modo | Propósito |
|-----|----------|-------------|------|------------|
| 🧩 **Parallel Scenarios (Chrome)** | Automático en cada push o PR | Chrome | `EXECUTION_MODE=PARALLEL` | Validación rápida y ligera |
| 🌐 **Cross-Browser MultiDriver** | Manual (`workflow_dispatch`) | Chrome, Firefox, Edge | `EXECUTION_MODE=MULTI` | Pruebas simultáneas cross-browser |

📦 Los resultados de ambos se almacenan como artefactos (`TestResults-Parallel`, `TestResults-Multi`).

---

## 🧾 Versionado automático

El pipeline **`.github/workflows/semantic-version.yml`** gestiona versiones siguiendo semver (`vX.Y.Z`):

- 🏷️ Crea y empuja un nuevo tag (`v1.2.4`, por ejemplo)  
- 📝 Actualiza el archivo `VERSION`  
- 📘 Actualiza el `README.md` con la versión actual y badge  
- 🚀 Mantiene histórico mediante commits automáticos  
- 🧮 Detecta cambios de tipo *major*, *minor* o *patch* según mensajes de commit

---

## 🧩 Ejemplo de ejecución (Parallel)

```
[unknown][Thread 18][12:21:38] [LOCAL MODE] Navegadores detectados: chrome, firefox, edge
[unknown][Thread 18][12:21:38] Escenario: 'Successful login' | EXECUTION_MODE=PARALLEL | headless=False
[unknown][Thread 18][12:21:38] [PARALLEL MODE] Creando drivers en paralelo: chrome, firefox, edge

[chrome][Thread 32][12:21:39] Driver iniciado (headless=False)
[edge][Thread 28][12:21:39] Driver iniciado (headless=False)
[firefox][Thread 26][12:21:44] Driver iniciado (headless=False)

[chrome][Thread 18][12:21:44] Driver inicializado para 'chrome' (headless=False)

Given I am on the login page
-> done: LoginSteps.GivenIAmOnLoginPage() (3,9s)

When I enter valid credentials
-> done: LoginSteps.WhenIEnterValidCredentials() (1,2s)

And I click the login button
-> done: LoginSteps.WhenIClickLoginButton() (0,0s)

Then I should see the dashboard
-> done: LoginSteps.ThenIShouldSeeTheDashboard() (0,0s)

[chrome][Thread 18][12:21:49] Cerrando navegador...
[edge][Thread 28][12:21:49] Cerrando navegador...
[firefox][Thread 26][12:21:49] Cerrando navegador...

```

---

## 📈 Próximos pasos

- [ ] GitHub Actions: Solve ChromeDriver v133+ issue on CI (user data directory bug)
- [x] **SonarQube**: Quality Gate pasado ✅ (coverage & maintainability)
- [ ] Mejorar sistema de logging (NLog / Serilog)  
- [ ] Generar reportes visuales con **SpecFlow+ LivingDoc**  
- [ ] Añadir **tags diferenciados** para smoke y regresión  
- [ ] Integrar **métricas de rendimiento** (tiempos por Step / Escenario)
- [ ] Implementar patrón **Core Layer (Component / Element Layer)** completo  

---

## 🧪 Estado actual

> Este proyecto es una **PoC experimental** enfocada en paralelismo, rendimiento y compatibilidad multi-navegador.  
> Puede servir como base para futuros frameworks BDD más avanzados en entornos CI/CD reales.

Current version: **v0.0.1**

🔗 ![Build passing](docs/pipeline-passing.png)

---

📘 Licencia: MIT © Rubén  
_Contribuciones y forks bienvenidos mientras se mantenga el propósito experimental._

