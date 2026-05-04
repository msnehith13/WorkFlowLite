# WorkflowLite

**A no-code business process automation engine** — built with ASP.NET Core, C#, React, and SQLite.  
Directly inspired by [Decisions](https://decisions.com), a leading BPA platform used by Fortune 500 companies.

---

## What It Does

WorkflowLite lets users define, trigger, and track multi-step automated business processes — without writing process logic manually. Think of it as a mini version of an enterprise workflow engine:

- Define a workflow with ordered steps (Assign → Approve → Notify → Complete)
- Attach rules to each step (`if status equals "approved" → proceed`)
- Trigger workflows with a JSON context payload
- The rules engine evaluates conditions and executes steps automatically
- Every run is logged with a full audit trail per step

---


## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 10, C# |
| Auth | JWT Bearer tokens |
| ORM | Entity Framework Core 9 + SQLite |
| Frontend | React 18 + Vite + Plain CSS |
| HTTP Client | Axios |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Architecture | Clean Architecture (4-layer) |

---

## Architecture

This project follows **Clean Architecture** — each layer has a single responsibility and dependencies only point inward.

```
WorkflowLite/
├── WorkflowLite.Domain          # Pure C# models — no dependencies
│   ├── Entities/                # Workflow, Step, Rule, WorkflowInstance, StepLog
│   ├── Enums/                   # StepType, WorkflowStatus, RuleOperator
│   └── Interfaces/              # IWorkflowRepository, IUserRepository, etc.
│
├── WorkflowLite.Application     # Business logic — depends only on Domain
│   ├── Services/                # WorkflowService (rules engine), AuthService (JWT)
│   └── DTOs/                    # Request/response records
│
├── WorkflowLite.Infrastructure  # Data access — implements Domain interfaces
│   ├── Data/                    # AppDbContext, EF Core config
│   └── Repositories/            # WorkflowRepository, UserRepository, etc.
│
├── WorkflowLite.API             # HTTP layer — thin controllers, middleware
│   ├── Controllers/             # AuthController, WorkflowsController
│   └── Middleware/              # Global exception handler
│
└── WorkflowLite.Web             # React frontend — Vite + React Router + Axios
    └── src/
        ├── pages/               # Login, Register, Dashboard, CreateWorkflow, WorkflowDetail
        ├── components/          # Layout, ProtectedRoute
        └── api/                 # Axios client + endpoint modules
```

**Why Clean Architecture?**  
The rules engine and workflow execution logic live in `Application` — completely independent of EF Core, SQLite, or ASP.NET. This means they can be unit tested with mocked repositories, and the database can be swapped without touching business logic. This is the same pattern used in enterprise-scale BPA platforms.

---

## Rules Engine

The core of the project. Each step can have multiple rules — all must pass for the step to execute.

```csharp
// Rule evaluation — pure C#, no framework dependencies
private static bool EvaluateRule(Rule rule, Dictionary<string, string> context)
{
    if (!context.TryGetValue(rule.Field, out var actualValue)) return false;

    return rule.Operator switch
    {
        RuleOperator.Equals      => actualValue.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
        RuleOperator.NotEquals   => !actualValue.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
        RuleOperator.Contains    => actualValue.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
        RuleOperator.GreaterThan => double.TryParse(actualValue, out var a) && double.TryParse(rule.Value, out var b) && a > b,
        RuleOperator.LessThan    => double.TryParse(actualValue, out var a2) && double.TryParse(rule.Value, out var b2) && a2 < b2,
        _ => false
    };
}
```

**Trigger example** — POST `/api/workflows/{id}/trigger`:
```json
{
  "contextJson": "{\"status\":\"approved\",\"amount\":\"750\"}"
}
```

Steps with rules like `status equals approved` will execute. Steps whose rules fail are marked `Skipped` in the audit log. If any step throws, the instance is marked `Failed` at that step — preserving the full log up to that point.

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | ❌ | Register a new user |
| POST | `/api/auth/login` | ❌ | Login and receive JWT |
| GET | `/api/workflows` | ✅ | List all workflows |
| POST | `/api/workflows` | ✅ | Create workflow with steps + rules |
| GET | `/api/workflows/{id}` | ✅ | Get workflow details |
| PUT | `/api/workflows/{id}` | ✅ | Update workflow |
| DELETE | `/api/workflows/{id}` | ✅ | Delete workflow |
| POST | `/api/workflows/{id}/trigger` | ✅ | Execute workflow |
| GET | `/api/workflows/{id}/history` | ✅ | Get execution history |

Full interactive docs available at `/swagger` when running locally.

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org)
- [dotnet-ef tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clone the repo

```bash
git clone https://github.com/yourusername/WorkflowLite.git
cd WorkflowLite
```

### 2. Run the API

```bash
cd WorkflowLite.API
dotnet ef database update --project ../WorkflowLite.Infrastructure --startup-project .
dotnet run
```

API runs at `http://localhost:5197`  
Swagger UI at `http://localhost:5197/swagger`

### 3. Run the React frontend

```bash
cd WorkflowLite.Web
npm install
npm run dev
```

Frontend runs at `http://localhost:5173`

---

## Key Design Decisions

**Why not use ASP.NET Identity?**  
Identity adds significant complexity (role tables, claim stores, token providers) that would obscure the core workflow logic. A lightweight custom `AppUser` + BCrypt + JWT approach keeps the auth simple and the domain clean — and is easier to reason about in an interview context.

**Why SQLite?**  
Zero-configuration local setup. The EF Core `DbContext` and repository abstractions mean switching to SQL Server or PostgreSQL is a one-line connection string change — the rest of the app is unaffected.

**Why are rules stored as structured fields (not raw JSON expressions)?**  
Storing `Field`, `Operator`, and `Value` as typed columns makes rules queryable, auditable, and UI-renderable without parsing. A rules expression like `status equals approved` can be displayed in the UI, edited in a form, and evaluated in C# — all without a parser.

---



## About

Built as a portfolio project to demonstrate Clean Architecture, OOP design, and full-stack .NET + React development — directly mirroring the core product of [Decisions](https://decisions.com), a business process automation platform.

**Author:** Snehith Modalavalasa  
**LinkedIn:** [Snehith Modalavalasa](https://www.linkedin.com/in/snehith-modalavalasa-72428828a/)  
**Email:** msnehith13@gmail.com / snehithmodalavalasa05@gmail.com
