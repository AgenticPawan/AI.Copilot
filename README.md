# AI.Copilot.Scaffolding

## 📦 Overview
`AI.Copilot.Scaffolding` is a NuGet package that **auto‑generates a standardized Copilot workspace scaffold**.  
When installed, it creates a modular folder and file structure designed for enterprise‑grade AI development, ensuring every repository starts with a **production‑ready Copilot environment**.

---

## 🚀 Features
- **Auto‑generate directories** (`.copilot-chat`, `.github`, `.vscode`)
- **Create default files with content** (Markdown, JSON configs)
- **Non‑destructive**: existing files are preserved
- **Validation**: MSBuild checks enforce required headers and fields
- **CI/CD enforcement**: GitHub Action workflow ensures compliance

---

## 📂 Scaffold Structure

```
.copilot-chat/
  01-behaviour-layer/
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
    skills/backend-architect/SKILL.md
    workflows/COPILOT_QUICK_START.md
    copilot-instructions.md

.vscode/
  mcp.json
  settings.json
```

---

## 📥 Installation

### Using .NET CLI
```bash
dotnet add package AI.Copilot.Scaffolding --version 1.0.0
```

### Using Visual Studio / Rider
- Open **NuGet Package Manager**
- Search for `AI.Copilot.Scaffolding`
- Install into your project

---

## ⚙️ How It Works
- Package ships with a `.targets` file:
  - **Ensures directories exist**
  - **Writes default content** into missing files
  - **Validates file contents** (e.g., `coding-standards.md` must contain `## Coding Standards`)
- Runs automatically during build, so your scaffold is always enforced.

---

## 📖 Example File Contents

- **coding-standards.md**
  ```markdown
  ## Coding Standards
  Defines project-wide coding rules and conventions.
  - Use PascalCase for classes
  - Use camelCase for variables
  - Always include XML documentation for public methods
  ```

- **system-prompt.md**
  ```markdown
  ## System Prompt
  Defines Copilot system behavior and constraints.
  ```

- **mcp.json**
  ```json
  {
    "version": "1.0",
    "description": "MCP workspace config"
  }
  ```

---

## 🛡️ CI/CD Enforcement
Add this workflow to `.github/workflows/scaffold-check.yml`:

```yaml
name: Scaffold Compliance Check
on: [push, pull_request]
jobs:
  scaffold-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
```

This ensures every build validates scaffold presence and correctness.

---

## 🏆 Benefits
- **Consistency**: Every repo starts with the same scaffold.
- **Automation**: No manual setup required.
- **Compliance**: Standards enforced at build and CI/CD.
- **Portability**: Works in VS Code, Visual Studio, Rider, or any IDE supporting NuGet/MSBuild.

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
Created by **Pawan**  
Senior Architect & Prompt Engineer specializing in enterprise SaaS automation.

---
```

---
