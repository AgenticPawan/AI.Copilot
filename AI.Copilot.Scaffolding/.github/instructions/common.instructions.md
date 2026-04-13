---
applyTo: "**"
description: "Cross-cutting development standards for {{projectName}}. Applies to all files. Covers Angular + .NET API setup, production support, upgrade workflow, QA, code review, and Playwright automation."
---

# Cross-Cutting Development Standards — {{projectName}}

## 1. Angular + .NET API Setup
// Generate Angular project with MCP client integration  
// Scaffold .NET 10 Web API with MCP server endpoints  
// Configure Angular proxy to call .NET API MCP endpoints  

## 2. Production Support
// Add logging middleware in .NET API (Serilog)  
// Add Angular global error handler service  
// Generate health-check endpoint in .NET API  
// Scaffold monitoring dashboard in Angular (AdminLTE/Fortress style)  

## 3. Upgrade Workflow
// Write Angular CLI upgrade script with reproducible steps  
// Generate .NET migration script for upgrading from .NET 6/7/8 to .NET 10  
// Document upgrade checklist in Markdown  

## 4. QA & Functional Testing
// Scaffold Jest/Karma test for Angular component  
// Scaffold xUnit test for .NET controller  
// Generate CI/CD pipeline step for running QA tests  

## 5. Code Review Guidelines
// Generate ESLint + Prettier config for Angular  
// Generate StyleCop + SonarQube config for .NET API  
// Write Markdown checklist for code review best practices  

## 6. Requirement Analysis
// Generate template for requirement analysis in Markdown  
// Scaffold reproducible prompt for Copilot to analyze requirements  

## 7. Unit Testing
// Write Angular service unit test with Jasmine  
// Write .NET controller unit test with xUnit  

## 8. Playwright Automation Suite
// Scaffold Playwright test for Angular login flow  
// Capture screenshot at each step  
// Generate HTML report with screenshots  
// Integrate Playwright suite into CI/CD pipeline  

---

## 🔑 Usage Notes
- Place this file at the root of your repo.  
- Open it in VS Code and start typing the trigger comments (e.g., `// Scaffold Playwright test for Angular login flow`).  
- Copilot will autocomplete with **implementation-ready code**.  
- Iterate by refining prompts inline (e.g., “with Redis caching” or “with JWT authentication”).  
---