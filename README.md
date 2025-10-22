# SpecFlow Selenium Tests


![Version Badge](https://img.shields.io/badge/version-v0.0.1-blue?style=for-the-badge)


[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=bugs)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=rubenlopez77_SpecFlowSelenium&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=rubenlopez77_SpecFlowSelenium)

---

# 🧪 Playwright + Cucumber + TypeScript Automation Framework

Proyecto personal para **experimentar** con **Playwright + TypeScript**, con el objetivo de replicar la flexibilidad lograda previamente con Selenium, y explorar nuevas posibilidades en testing moderno orientado a mantenibilidad, paralelización y trazabilidad, incorporando además un enfoque experimental con **inteligencia artificial** para optimizar la generación, análisis y priorización de pruebas. 

(En proceso...)

---

## 🤖  Experimentación con IA

La integración de herramientas de inteligencia artificial (IA) en entornos de pruebas automatizadas puede aportar velocidad y asistencia en la generación de escenarios o casos de prueba.  

Sin embargo, la mayoría de las soluciones actuales de IA que generan código o tests a partir de descripciones en texto libre **no respetan las buenas prácticas de diseño QA** como el **Page Object Model (POM)** ni la **capa de componentes**.

El propósito de esta experimentación es **incluir IA como apoyo semántico y generativo**, sin comprometer la calidad ni la trazabilidad de las pruebas automatizadas.

---

## 📋 Índice
1. [Arquitectura del proyecto](#arquitectura-del-proyecto)
2. [Requisitos previos](#requisitos-previos)
3. [Instalación](#instalación)
4. [Ejecución de pruebas](#ejecución-de-pruebas)
5. [Configuración de entornos](#configuración-de-entornos)
6. [Estrategia de calidad](#buenas-prácticas)
6. [Capas de Abstracción y Arquitectura de Automatización](#buenas-prácticas) 
7. [Reportes y trazas](#reportes-y-trazas)
8. [Estructura de carpetas](#estructura-de-carpetas)
9. [Integración continua](#integración-continua)
10. [Roadmap](#roadmap)

---

## 🧱 Arquitectura del proyecto

El framework sigue el patrón **Page Object Model (POM)** y utiliza **fixtures reutilizables** para manejo de datos y contexto de pruebas.  
Los escenarios están definidos en **Gherkin (BDD)** para permitir colaboración entre QA, desarrollo y negocio.

```
Playwright (core) + Cucumber (BDD) + TypeScript (strong typing)
```

- **Playwright** → Ejecución de tests en múltiples navegadores.
- **Cucumber** → Escenarios BDD legibles por negocio.
- **TypeScript** → Tipado estático y calidad de código.
- **GitHub Actions** → Integración continua y generación de reportes.

---

## ⚙️ Requisitos previos

- Node.js >= 18  
- npm o yarn  
- Playwright CLI  
- Git

Instalar Playwright browsers (una sola vez):
```bash
npx playwright install
```

---

## 📦 Instalación

```bash
git clone https://github.com/rubenlopez77/Playwright_fun.git
cd Playwright_fun
npm install
```

---

## 🚀 Ejecución de pruebas

### Modo consola
```bash
npx playwright test
```

### Modo UI
```bash
npx playwright test --ui
```

### Ejecución BDD (Cucumber)
```bash
npx cucumber-js --require-module ts-node/register --require ./tests/steps/**/*.ts --format progress
```

---

## 🌐 Configuración de entornos

Variables sensibles se gestionan mediante ficheros `.env`.  
**No se versionan**, solo se provee un ejemplo genérico:

```bash
# .env.example
BASE_URL=https://
USER_EMAIL=test@example.com
USER_PASSWORD=secret
```

Selecciona entorno con:
```bash
ENV=qa npx playwright test
```

---

## 🧩 Estrategia de Calidad y Mejores Prácticas

- - **Page Objects:** una clase por página con acciones claras (`home()`, `login(user,pass)` etc  con el mismo nombre del botón o enlace.  “El código se lee como una historia.”
- **Selectors:** usar siempre `data-test` o atributos específicos del DOM.  
- **Fixtures:** inicializar datos y estados en `beforeAll` o `beforeEach`.  
- **Tests atómicos:** cada escenario debe validar un único flujo de negocio.  
- **Linting & Types:** ejecuta `npm run lint` y `npm run typecheck` antes de subir cambios.  
- **Commits limpios:** convención `feat/test/fix/chore`.  
- **Quality Gate con SonarQube:** define umbrales mínimos de cobertura, duplicación y deuda técnica antes de aceptar merges.  
- **Ejecución en paralelo y cross-browser:** aprovechar la capacidad nativa de Playwright para correr tests simultáneamente en **Chromium**, **Firefox** y **WebKit**.  
- **Alta reutilización de componentes:** promover abstracción y modularidad en fixtures, utilidades y Page Objects para minimizar duplicación y facilitar mantenimiento.  
- **AI-assisted QA:** explorar el uso de inteligencia artificial en **generación automática de tests**, **análisis de logs** y **detección de patrones de fallos** para optimizar la cobertura y reducir el tiempo de diagnóstico.

---

## 🧱 Capas de Abstracción y Arquitectura de Automatización

El framework sigue una **arquitectura multicapa** basada en el patrón **Page Object Model (POM)** y en principios de **bajo acoplamiento y alta cohesión**, de acuerdo con las recomendaciones de **ISTQB** para frameworks de automatización sostenibles.

🧬 **Estructura de capas**

Helper de componente → Helper de página (POM) → Prueba (feature / test)

Este ejemplo muestra cómo una prueba sencilla de login utiliza la arquitectura propuesta, separando responsabilidades entre la prueba, el helper de página y los helpers de componentes.

#### 🧩 1. Helper de componente
- Contiene la lógica de interacción con **elementos** de la web (botones, inputs, selectores, modales, tooltips, etc.).
- Se encarga de las esperas (`await expect(...)`), selectores y validaciones específicas de ese componente.

(En desarrollo)

#### 🧩 2. Helper de página (POM)
- Representa una página completa o una vista funcional.
- **No repite lógica de bajo nivel**, sino que **utiliza los helpers de componentes** para mantener la capa limpia.
- Define métodos con el mismo nombre visible en la web:  

  ```typescript

	public async doLogin(user: string, pass:string, success : boolean =true): Promise<void> {
		const loginModal = this.page.locator('#logInModal');
	
	    const usernameField = this.page.locator('#loginusername');
	    const passwordField = this.page.locator('#loginpassword');
	
	    await expect(usernameField).toBeVisible();
	    await expect(passwordField).toBeVisible();
	
	    await usernameField.fill(user); 
	    await passwordField.fill(pass);
	
	    if (success) {
	      await loginModal.locator('button',{ hasText: 'Log in' }).click();
		}
	}

#### 🧪 3. Prueba 
- El test es **declarativo**, solo indica *qué* se valida, no *cómo*.

  ```typescript

  test('should fail login with invalid credentials', async ({ page }) => { 
    const login = new Login(page);
    await login.doLogin("login", "KO", false);
  });

####  🥒 4. Escenario BDD (Cucumber)
El nivel más alto de abstracción: describe el comportamiento del usuario en lenguaje natural, sin código técnico

```gherkin
Feature: Login functionality
  In order to access the application
  As a registered user
  I want to log in and handle errors correctly

  Scenario Outline: Unsuccessful login
    Given I am on the login page
    When I enter invalid credentials
    Then I should see the error message
```

## 🧾 Reportes y trazas 

Tras cada ejecución se genera automáticamente:
- **HTML Report:** `/playwright-report/index.html`
- **Trace Viewer:** `/test-results/**/trace.zip`
- **Screenshots & Videos:** capturados en fallos

Para abrir el reporte:
```bash
npx playwright show-reportEn CI se publican como artefactos automáticamente.

---

## 📂 Estructura de carpetas

```bash
Playwright_fun/
├── .github/workflows/           # Pipelines de CI/CD
├── .vscode/                     # Configuración del entorno de desarrollo
├── tests/
│   ├── features/                # Escenarios Gherkin (.feature)
│   ├── steps/                   # Definiciones de pasos de Cucumber
│   ├── helpers /
│   │   ├── pages/               # Page Objects
│   │   ├── fixtures/            # Fixtures y hooks comunes
│   │   └── utils/               # Funciones utilitarias
│   └── data/                    # Datos estáticos o JSON de prueba
├── playwright.config.ts         # Configuración global de Playwright
├── cucumber.js                  # Configuración de Cucumber
├── package.json                 # Scripts, dependencias y comandos
├── .env.example                 # Plantilla de configuración de entorno
└── README.md
```

---

## 🧩 Integración continua

Pipeline automatizado con **GitHub Actions** que ejecuta:
1. Lint & type check  
2. Ejecución de tests en matrix (Chrome, Firefox, WebKit)  
3. Generación de reportes HTML + trazas  
4. Publicación de artefactos (`playwright-report`, `traces`, `videos`)

```yaml
# .github/workflows/ci.yml
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 18
      - run: npm ci
      - run: npx playwright install --with-deps
      - run: npx playwright test --browser ${{ matrix.browser }}
      - uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: playwright-report
```

---

## 🧭 Roadmap
- [ ] Implementar Helper de **componentes**
- [x]  SonarQube Quality Gate ✅
- [ ]  **BDD:** Escenarios escritos en **Gherkin (.feature)**
- [ ] Conectar con pipelines de despliegue  
- [ ] **Añadir** tests de regresión completa  
- [ ] Integrar **allure-report**
- [ ] Generar coverage report  
- [ ] Integrar con Slack / Notificaciones CI
- [ ] Añadir pruebas visuales y de **accesibilidad**

---

## 👨‍💻 Autor

**Rubén López**  
🧑‍🔬 QA Senior 📦 [GitHub](https://github.com/rubenlopez77)🔗 [LinkedIn](https://www.linkedin.com/in/ruben-lopez-qa/)

---
