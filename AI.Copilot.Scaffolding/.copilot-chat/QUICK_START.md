# VS Code Copilot Chat: Quick Start Guide

How to use the Copilot Copilot Chat System in VS Code.

---

## 🚀 Getting Started

### Access Copilot Chat in VS Code

```
Keyboard Shortcut: Ctrl + Shift + I
Or: Click Copilot icon in left sidebar
Or: Cmd Palette (Ctrl+Shift+P) → "Copilot Chat: Open"
```

### Three Ways to Use the System

#### 1️⃣ Copy-Paste Approach (Quick)
Most straightforward - copy relevant section from `.copilot-chat` files and paste into chat.

#### 2️⃣ File Reference Approach (Organized)
Reference files by name, ask Claude to read them.

#### 3️⃣ Context Loading Approach (Comprehensive)
Start first message loading all context, then subsequent messages reuse the context.

---

## 📋 Quick Recipes

### Recipe 1: Add a New Backend Feature

```
Copilot Chat Messages:

1️⃣ "Read .copilot-chat/01-behaviour-layer/system-prompt.md
   and .copilot-chat/01-behaviour-layer/architecture-guide.md
   to understand the project context."

2️⃣ "Follow .copilot-chat/02-task-layer/backend-feature.md
   and act as the Backend Architect from personas.md
   to help me design a new feature for managing Documents.

   Requirements:
   - CRUD operations (Create, Read, Update, Delete)
   - Tenant-scoped (each tenant has their own documents)
   - User can view but only managers can edit
   - Auto-track creation/modification by user"

3️⃣ "Now let's implement it step by step. First, create the Entity class
   following the factory pattern shown in architecture-guide.md"

4️⃣ "Now generate the Application layer (DTOs, Commands, Handlers, Validators)
   using the pattern from backend-feature.md"

5️⃣ "Now create the API endpoints with proper authorization"

6️⃣ "Now write unit tests following unit-testing.md"
```

### Recipe 2: Fix a Production Bug

```
Copilot Chat Messages:

1️⃣ "An admin user reported that document search returns
   documents from other tenants! This is a CRITICAL security issue.

   Act as the Debugging Expert from personas.md
   and help me diagnose using bug-fix.md methodology."

2️⃣ "I found the issue - the repository query doesn't filter by TenantId.
   Help me fix it and write a test case that reproduces the bug first."

3️⃣ "The test now fails as expected (confirming the bug).
   Now let me fix the repository method... [paste fixed code]

   Does this look correct?"

4️⃣ "Now run full regression tests and verify the fix.
   What else could be affected by this bug?"
```

### Recipe 3: Create a New Frontend Component

```
Copilot Chat Messages:

1️⃣ "I need to create a new feature in the frontend.
   Load context from:
   - .copilot-chat/01-behaviour-layer/system-prompt.md
   - .copilot-chat/02-task-layer/frontend-feature.md

   Act as the Frontend Specialist from personas.md"

2️⃣ "I need to create a document list page with:
   - Angular 20 standalone component
   - OnPush change detection
   - Load documents from API
   - Show pagination
   - Permission-based delete button
   - Proper error handling and loading states

   Generate the complete component code."

3️⃣ "Now generate the service to call the backend API"

4️⃣ "Now generate the routing setup"

5️⃣ "Now write unit tests for the component"
```

### Recipe 4: Review Code Quality

```
Copilot Chat Messages:

1️⃣ "Act as the Mentor from personas.md.
   Read coding-standards.md and review this code:

   [paste your code here]

   What improvements would you suggest?"

2️⃣ "Now act as the Security Specialist.
   Review the same code for security concerns."

3️⃣ "Now act as the Performance Optimizer.
   Are there any performance issues?"
```

### Recipe 5: Understand a Complex Feature

```
Copilot Chat Messages:

1️⃣ "I need to understand how multi-tenancy works in this project.
   Read architecture-guide.md and explain the Multi-Tenancy Flow
   in simple terms for someone new to the project."

2️⃣ "Now show me a code example of correct vs incorrect
   multi-tenant filtering in repositories."

3️⃣ "What are common mistakes developers make?"
```

---

## 🎯 Multi-Turn Conversation Strategy

**Best Practice**: Build context in first message, then ask specific questions.

```
Turn 1 (Setup Context):
"I'm working on Copilot.
 Context files:
 - .copilot-chat/01-behaviour-layer/system-prompt.md
 - .copilot-chat/01-behaviour-layer/architecture-guide.md

 I'll be asking follow-up questions about feature development."

Turn 2+ (Specific Questions):
"Now, help me create an entity for [Feature]"
"Now, write the command handler..."
etc.

Benefit: AI remembers entire context from Turn 1
```

---

## 💡 Pro Tips

### Tip 1: Use Personas Explicitly
```
❌ "How do I optimize this query?"
✅ "Act as the Performance Optimizer from profiles.md.
   Here's a slow query occurring in document listing.
   How would you optimize it?"
```

### Tip 2: Reference Specific Files
```
❌ "How should I structure this?"
✅ "Follow the pattern in architecture-guide.md → Repository Pattern section.
   Here's my current code... how should I refactor?"
```

### Tip 3: Ask for Multiple Perspectives
```
"Act as the Backend Architect and tell me if this design is good.
 Then act as the Security Specialist and review for security.
 Finally act as the Performance Optimizer and identify bottlenecks."
```

### Tip 4: Use Code Blocks
```
When pasting code in chat, use markdown code blocks:

```csharp
// Your code here
```

This helps Claude understand the language and syntax.
```

### Tip 5: Ask for Specific Format
```
Instead of:
"Write unit tests"

Ask:
"Write xUnit unit tests with Moq mocks
 following the patterns in unit-testing.md
 with AAA (Arrange-Act-Assert) structure"
```

### Tip 6: Iterative Refinement
```
Message 1: "Generate a basic solution"
Message 2: "That's good, but can you add error handling?"
Message 3: "Now can you also add logging?"
```

---

## 🔍 Finding the Right File

| Task | File | Section |
|------|------|---------|
| Understanding project | system-prompt.md | Core Principles |
| New backend feature | backend-feature.md | Full file |
| New frontend feature | frontend-feature.md | Full file |
| Debugging issue | bug-fix.md | Debugging Methodology |
| Writing tests | unit-testing.md | Full file |
| API design | api-integration.md | Full file |
| Stuck on problem | personas.md | Choose persona |
| Multi-step complex task | workflows.md | Find matching workflow |
| Code standards | coding-standards.md | Relevant section |
| Architecture questions | architecture-guide.md | Specific layer |

---

## 🎓 Example Session: Complete New Feature

**Goal**: Add a "FamilyMember" feature (tenant-scoped CRUD)

```
---MESSAGE 1---
Load this context from the .copilot-chat directory:
- 01-behaviour-layer/system-prompt.md
- 01-behaviour-layer/architecture-guide.md
- 02-task-layer/backend-feature.md

Ready to design a new feature?

---MESSAGE 2---
I'm adding a FamilyMember feature. Requirements:
- Each tenant can manage their own family members
- CRUD operations (Create, Read, Update, Delete)
- Auto-track who created/modified
- Managers can view/edit, users can only view their own
- Database table: FamilyMembers (Id, FirstName, LastName, RelationshipType, TenantId, CreatedByUserId, etc.)

Act as the Backend Architect and give me the design walkthrough
following backend-feature.md workflow.

---MESSAGE 3---
Great! Now generate the Entity code (Step 1 of backend-feature.md)

---MESSAGE 4---
Now generate the DTOs (Step 2)

---MESSAGE 5---
Now generate the Command and Handler (Step 3)

---MESSAGE 6---
Now generate the Repository interface and implementation (Step 4)

---MESSAGE 7---
Now generate the API Controller (Step 6)

---MESSAGE 8---
Now generate the EF Core configuration (Step 7)

---MESSAGE 9---
Now write unit tests for the CommandHandler (Step 8)

---MESSAGE 10---
Act as the Security Specialist.
Review this design for security. Are all the requirements met?
- Permission checks
- Multi-tenant isolation
- Input validation

---MESSAGE 11---
Now I'll create the frontend.
Switch to acting as the Frontend Specialist.
Help me create the list component, service, and routing
following frontend-feature.md

---MESSAGE 12---
Write unit tests for the component

---MESSAGE 13---
Review the entire feature from all angles:
- Backend Architect: Is design solid?
- Security Specialist: Are we secure?
- QA Engineer: What should we test?
- Performance Optimizer: Any perf concerns?
```

**Result**: Complete, well-designed feature with tests and security review ✅

---

## ❌ Common Mistakes to Avoid

```
❌ Not providing context
   "How do I write tests?"

✅ "We use xUnit + Moq for backend tests.
   Show me an example following unit-testing.md"

---

❌ Asking too broadly
   "How should I structure my code?"

✅ "Following architecture-guide.md,
   how should I structure the repository layer?"

---

❌ Not specifying project type
   "Write a test"

✅ "Write a C# xUnit test following
   the patterns in coding-standards.md backend section"

---

❌ Ignoring multi-tenancy
   "Write a query to get all documents"

✅ "Write a repository method to get all documents
   for the current tenant, ensuring multi-tenant isolation"

---

❌ Not reading error messages
   "My code doesn't work"

✅ "I got this error: [exact error message].
   Here's my code: [code]. What's wrong?"
```

---

## 🎯 When to Use Each File

### system-prompt.md
- When establishing context
- When confused about project principles
- When reviewing code for standards

### architecture-guide.md
- When designing new layers
- When understanding existing patterns
- When stuck on "where does this go?"

### coding-standards.md
- When writing code
- When reviewing code
- When improving code quality

### backend-feature.md
- Literally every backend feature
- Follow step-by-step

### frontend-feature.md
- Literally every frontend feature
- Follow step-by-step

### bug-fix.md
- Production issues
- Failing tests
- Unexpected behavior

### unit-testing.md
- Writing new tests
- Improving test coverage
- Understanding test patterns

### api-integration.md
- Designing API endpoints
- Versioning strategy
- External service integration

### personas.md
- Choosing right perspective
- Getting specialized advice
- Code review

### workflows.md
- Complex multi-step tasks
- Uncertain how to proceed
- Learning best practices

---

## 📞 Still Need Help?

**If Copilot doesn't understand:**
1. Check you used the right file reference
2. Be more specific about what you need
3. Paste exact error messages
4. Ask a different way

**If answer seems wrong:**
1. Cross-reference with project docs
2. Test the solution
3. Ask Copilot to reconsider
4. Get second opinion from team

**If workflow is unclear:**
1. Follow the step-by-step in workflows.md
2. Ask Copilot to explain each step
3. Read the referenced architecture files
4. Link to real examples in the codebase

---

**Last Updated**: 2026-04-01 | For Copilot v1.0
