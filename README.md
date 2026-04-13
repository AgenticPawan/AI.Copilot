# AI.Copilot.Scaffolding

## 📦 Overview
`AI.Copilot.Scaffolding` is a **dotnet new template package** that **scaffolds a standardized Copilot workspace** into any directory.  
When used, it creates a modular folder and file structure designed for enterprise‑grade AI development, ensuring every repository starts with a **production‑ready Copilot environment**.

**Works with:** .NET projects, Angular projects, or any empty folder — no build system required.

---

## 🚀 Features
- **Instant scaffolding** — creates all directories and files with rich content in one command
- **Framework‑agnostic** — works in .NET, Angular, Node.js, or any project type
- **Empty folder support** — scaffold into a brand new directory
- **Rich template content** — full Copilot workspace layers with agents, instructions, prompts, and skills
- **Non‑destructive** — use `--force` only when you want to overwrite existing files
- **CI/CD enforcement** — GitHub Action workflow ensures compliance

---

## 📂 Scaffold Structure

```
.copilot-chat/
  01-behaviour-layer/
    architecture-guide.md
    coding-standards.md
    system-prompt.md
  02-task-layer/
    api-integration.md
    backend-feature.md
    bug-fix.md
    frontend-feature.md
    unit-testing.md
  03-persona-layer/
    personas.md
  04-workflow-layer/
    workflows.md
    QUICK_START.md
    README.md

.github/
  agents/
    backend-architect.agent.md
    devops-engineer.agent.md
    frontend-engineer.agent.md
    fullstack-architect.agent.md
    qa-engineer.agent.md
  instructions/
    backend.instructions.md
    common.instructions.md
    frontend.instructions.md
    multi-tenancy.instructions.md
    security.instructions.md
    testing.instructions.md
  prompts/
    backend-add-handler.prompt.md
    backend-migration.prompt.md
    backend-new-feature.prompt.md
    bug-fix.prompt.md
    code-review.prompt.md
    frontend-add-component.prompt.md
    frontend-new-page.prompt.md
    requirements-analysis.prompt.md
    test-e2e-playwright.prompt.md
    test-functional.prompt.md
    test-unit-backend.prompt.md
    workflow-bug-resolution.prompt.md
    workflow-enhance-feature.prompt.md
    workflow-fullstack-feature.prompt.md
    workflow-tenant-provisioning.prompt.md
  skills/
    backend-architect/SKILL.md
  workflows/
    scaffold-check.yml
    COPILOT_QUICK_START.md

  copilot-instructions.md
  COPILOT_QUICK_START.md

.vscode/
  mcp.json
  settings.json
```

---

## 📥 Installation

### Step 1: Install the template (one-time)

```bash
dotnet new install AI.Copilot.Scaffolding
```

### Step 2: Scaffold into your project

#### Into an existing .NET project
```bash
cd your-dotnet-project
dotnet new copilot-scaffold
```

#### Into an existing Angular project
```bash
cd your-angular-project
dotnet new copilot-scaffold
```

#### Into an empty folder
```bash
mkdir my-new-project
cd my-new-project
dotnet new copilot-scaffold
```

### Overwrite existing files
```bash
dotnet new copilot-scaffold --force
```

### Uninstall the template
```bash
dotnet new uninstall AI.Copilot.Scaffolding
```

---

## ⚙️ How It Works
- The package is a **`dotnet new` template** (not a build-time NuGet dependency)
- Running `dotnet new copilot-scaffold` **physically copies** all template files with rich content into the current directory
- Files include full documentation, agent definitions, instruction sets, prompts, skills, and VS Code configuration
- No MSBuild, no build step — works anywhere the .NET SDK is available

---

## 📖 Usage Examples

### Scaffold into a new .NET Web API project
```bash
mkdir MyApi && cd MyApi
dotnet new webapi
dotnet new copilot-scaffold
```

### Scaffold into a new Angular project
```bash
ng new my-angular-app
cd my-angular-app
dotnet new copilot-scaffold
```

### Scaffold into a blank workspace
```bash
mkdir my-workspace && cd my-workspace
dotnet new copilot-scaffold
code .
```

---

## 🛡️ CI/CD Enforcement
The scaffold includes `.github/workflows/scaffold-check.yml` which validates that all required scaffold files are present on every push and pull request.

---

## 🏆 Benefits
- **Consistency**: Every repo starts with the same scaffold.
- **Zero config**: One command creates everything — no build, no restore.
- **Framework‑agnostic**: .NET, Angular, React, Node.js — any project type.
- **Rich content**: Full Copilot workspace layers, not empty stubs.
- **Portability**: Works in VS Code, Visual Studio, Rider, or any IDE.

---

## 📌 Versioning
- Semantic versioning (`1.0.0`, `1.1.0`, etc.)
- Breaking changes increment **major version**
- New scaffold files increment **minor version**

---

## 🔒 Security
- No sensitive data included
- Config files (`mcp.json`, `settings.json`) are safe defaults
- Recommended: sign package for enterprise distribution

---

## 👤 Author
Created by **Pawan Rahate**  
Aspiring Senior Full-Stack Lead & Prompt Engineer specializing in enterprise SaaS automation.

---
