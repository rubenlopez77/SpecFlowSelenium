# SpecFlow Multi-Browser Parallel Test Suite (Experimental)

## Overview

**WIP!!** "Este proyecto es una suite de automatizaci�n **experimental** creada para explorar **SpecFlow con Selenium**, ejecutando pruebas en paralelo a trav�s de m�ltiples navegadores.

- Run tests **in parallel** on Chrome, Firefox, and Edge.  
- Control browser selection and headless mode via a `.env` file.  
- Separate **smoke tests** and **regression tests** using tags.  
- Integrate with **CI/CD pipelines** (GitHub Actions / Azure DevOps).  


## Objetivos

Clean Login Steps - Simplifica la escritura de pruebas potenciando SpecFlow. Permite la escritura de pruebas usando lenguaje natural. 

Helpers � Centraliza las interacciones con el navegador para reducir la duplicaci�n de c�digo. Simplifica el mantenimiento y lo hace escalable.

Parallel Execution � Utiliza DriverFactory para lanzar m�ltiples navegadores en paralelo  y thread-safe.

Logs & Reporting (WIP) � Incluye m�tricas de ejecuci�n.

Environment Flexibility � .env o variables de CI/CD permiten controlar sin esfuerzo navegadores, entornos y modos.

Tagging Strategy � Flexibilidad al poder definir el tipo de pruebas al lanzar (Smoke y Regresi�n) Velocidad vs Cobertura en los pipelines de CI. Las pruebas Smoke por ejemplo s�lo se lanzan en Chrome

---

## Tecnolog�a

- [SpecFlow](https://specflow.org/) (BDD framework for .NET)  
- [Selenium WebDriver](https://www.selenium.dev/) for browser automation  
- [NUnit](https://nunit.org/) as the test runner  
- [.NET 8](https://dotnet.microsoft.com/)  
- Parallel execution using NUnit and custom `DriverFactory`  

---

## Estructura


- **Features/** � SpecFlow `.feature` files with scenarios.  
- **StepDefinitions/** � Step definitions linking Gherkin steps to Selenium actions.  
- **Helpers/** � Page Objects for reusable actions (e.g., `HomePage.Login()`).  
- **Helpers/** � Centralized driver initialization, multi-browser support, and parallel execution (`DriverFactory`).  
- **.env** � Environment configuration for browsers and headless mode.  

---

## Ejemplo Azure DevOps Pipeline 

```yaml
trigger:
- main
- dev

pool:
  vmImage: 'windows-latest'

variables:
  BROWSERS: 'chrome,firefox,edge'
  HEADLESS: 'true'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '9.x'
- script: dotnet restore
- script: dotnet test --filter "Category=Smoke"
  displayName: 'Run smoke tests in parallel'
  env:
    BROWSERS: $(BROWSERS)
    HEADLESS: $(HEADLESS)