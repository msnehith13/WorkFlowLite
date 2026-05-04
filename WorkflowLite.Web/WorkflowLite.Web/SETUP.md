# WorkflowLite — Phase 1 Setup Guide

## Prerequisites

- .NET 8 SDK
- VS Code with C# Dev Kit extension
- Node.js 18+

---

## 1. Run the scaffold commands

(See the terminal commands from the previous step)

## 2. Install extra NuGet packages

```bash
# BCrypt for password hashing (in Application project)
dotnet add WorkflowLite.Application/WorkflowLite.Application.csproj package BCrypt.Net-Next

# EF Core tools for migrations
dotnet tool install --global dotnet-ef
```

## 3. Create the first migration

```bash
cd WorkflowLite.API
dotnet ef migrations add InitialCreate --project ../WorkflowLite.Infrastructure --startup-project .
dotnet ef database update --project ../WorkflowLite.Infrastructure --startup-project .
```

## 4. Run the API

```bash
cd WorkflowLite.API
dotnet run
```

API runs at: http://localhost:5000
Swagger UI at: http://localhost:5000/swagger

---

## 5. Test with Swagger — in this order

1. POST /api/auth/register → create your account
2. POST /api/auth/login → copy the token
3. Click "Authorize" in Swagger, paste: Bearer <your_token>
4. POST /api/workflows → create a workflow
5. POST /api/workflows/{id}/trigger → run it

### Example trigger body

```json
{
  "contextJson": "{\"status\":\"approved\",\"amount\":\"500\"}"
}
```

---

## Project structure created

```
WorkflowLite/
├── WorkflowLite.Domain/
│   ├── Entities/        Workflow, Step, Rule, WorkflowInstance, StepLog, AppUser
│   ├── Enums/           StepType, WorkflowStatus, StepStatus, RuleOperator
│   └── Interfaces/      IWorkflowRepository, IUserRepository, etc.
│
├── WorkflowLite.Application/
│   ├── DTOs/            All request/response records
│   └── Services/        AuthService, WorkflowService (rules engine inside)
│
├── WorkflowLite.Infrastructure/
│   ├── Data/            AppDbContext with full EF Core config
│   └── Repositories/    All 4 repository implementations
│
└── WorkflowLite.API/
    ├── Controllers/     AuthController, WorkflowsController
    ├── Middleware/      ExceptionHandlingMiddleware
    └── Program.cs       Full DI, JWT, Swagger, CORS, auto-migrate
```
