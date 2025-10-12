# SpecFlow Selenium Tests

![Version Badge](https://img.shields.io/badge/version-v1.2.4-blue?style=for-the-badge)

Current version: **v1.2.4**

---

⚠️ **Prueba de concepto (PoC)** — Este proyecto es **experimental** y se utiliza únicamente para demostrar las capacidades de **SpecFlow + Selenium** en escenarios de ejecución **paralela** y **multi-navegador**.  
Su propósito es probar la **robustez**, el **aislamiento de contextos** y la **eficiencia en pipelines CI/CD**.  
No está destinado a entornos productivos.

---

## 🚀 Objetivo

Demostrar cómo implementar pruebas automatizadas **BDD (Behavior Driven Development)** con:

- ✅ Escenarios escritos en **Gherkin (.feature)**  
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
[setup][Thread 16][10:51:09] BROWSERS detectados: chrome, firefox, edge
[setup][Thread 16][10:51:09] Escenario: 'Successful login'
[setup][Thread 16][10:51:09] EXECUTION_MODE=PARALLEL
[setup][Thread 16][10:51:09] Lanzando escenario en un único navegador: chrome
[chrome][Thread 16][10:51:10] Driver iniciado (headless=False)
Given I am on the login page
-> done: LoginSteps.GivenIAmOnLoginPage() (1,2s)
When I enter valid credentials
-> done: LoginSteps.WhenIEnterValidCredentials() (0,3s)
Then I should see the dashboard
-> done: LoginSteps.ThenIShouldSeeTheDashboard() (0,0s)
[chrome][Thread 16][10:51:12] Cerrando navegador...
```

---

## 📈 Próximos pasos

- [ ] Implementar patrón **Page Object** completo  
- [ ] Mejorar sistema de logging (NLog / Serilog)  
- [ ] Generar reportes visuales con **SpecFlow+ LivingDoc**  
- [ ] Añadir **tags diferenciados** para smoke y regresión  
- [ ] Integrar **métricas de rendimiento** (tiempos por Step / Escenario)

---

## 🧪 Estado actual

> Este proyecto es una **PoC experimental** enfocada en paralelismo, rendimiento y compatibilidad multi-navegador.  
> Puede servir como base para futuros frameworks BDD más avanzados en entornos CI/CD reales.

---

📘 Licencia: MIT © Rubén  
_Contribuciones y forks bienvenidos mientras se mantenga el propósito experimental._
ñ