# SpecFlow Selenium Tests


![Version Badge](https://img.shields.io/badge/version-v0.0.1-blue?style=for-the-badge)

---

⚠️ **Prueba de concepto (PoC)** — Este proyecto es **experimental** y se utiliza únicamente para demostrar las capacidades de **SpecFlow + Selenium** en escenarios de ejecución **paralela** y **multi-navegador**.  
Su propósito es probar la **robustez**, el **aislamiento de contextos** y la **eficiencia en pipelines CI/CD**.  
No está destinado a entornos productivos.

---

## 🚀 Objetivo

Demostrar cómo implementar pruebas automatizadas **BDD (Behavior Driven Development)** con:

- ✅ BDD: Escenarios escritos en **Gherkin (.feature)**  
- ✅ Definiciones de pasos en **C# (SpecFlow Steps)**  
- ✅ Ejecución en paralelo con **NUnit [ParallelScope.Fixtures]**  
- ✅ Pruebas **cross-browser simultáneas** con arquitectura MultiDriver  
- ✅ Integración con **Selenium WebDriver** para interacción real con el navegador  
- ✅ **Helpers y Page Objects** reutilizables para reducir duplicación  
- ✅ **Estrategia de tags**: diferenciar *Smoke* y *Regresión* (velocidad vs cobertura)

---

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

---

## 🧠 Arquitectura técnica

El sistema utiliza un modelo híbrido:

| Elemento | Descripción |
|-----------|--------------|
| **DriverFactory** | Crea y gestiona instancias `IWebDriver` aisladas por hilo o escenario. |
| **ThreadLocal** | Aísla contexto de Selenium y ScenarioContext por hilo. |
| **EXECUTION_MODE** | Controla el comportamiento (`PARALLEL` o `MULTI`). |
| **Page Objects** | Encapsulan la lógica de interacción con la UI. |
| **Debug.Log** | Muestra `[browser][Thread][HH:mm:ss]` en consola, thread-safe. |

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
[unknown][Thread 15][10:45:45] [LOCAL MODE] Navegadores detectados: chrome, firefox, edge
[unknown][Thread 15][10:45:45] Escenario: 'Successful login'  |  EXECUTION_MODE=PARALLEL  |  headless=False
[chrome][Thread 15][10:45:46] [chrome][Thread 15] Driver iniciado (headless=False)
[firefox][Thread 15][10:45:51] [firefox][Thread 15] Driver iniciado (headless=False)
[edge][Thread 15][10:45:53] [edge][Thread 15] Driver iniciado (headless=False)
[chrome][Thread 15][10:45:53]  Driver inicializado para 'chrome' (headless=False)
Given I am on the login page
-> done: LoginSteps.GivenIAmOnLoginPage() (6,8s)
When I enter valid credentials
-> done: LoginSteps.WhenIEnterValidCredentials() (0,9s)
And I click the login button
-> done: LoginSteps.WhenIClickLoginButton() (0,0s)
Then I should see the dashboard
-> done: LoginSteps.ThenIShouldSeeTheDashboard() (0,0s)
[chrome][Thread 15][10:46:00] [chrome][Thread 15] Cerrando navegador...
[chrome][Thread 15][10:46:00] [chrome][Thread 15] Cerrando navegador...
[chrome][Thread 15][10:46:01] [chrome][Thread 15] Cerrando navegador...

```

---

## 📈 Próximos pasos

- [ ] GitHub Actions: Solve ChromeDriver v133+ issue on CI (user data directory bug) 
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
