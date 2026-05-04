using WorkflowLite.Domain.Enums;

namespace WorkflowLite.Application.DTOs;

// ── Auth ──────────────────────────────────────────────
public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string UserId, string FullName, string Email);

// ── Workflow ──────────────────────────────────────────
public record CreateWorkflowRequest(string Name, string Description, List<CreateStepRequest> Steps);
public record UpdateWorkflowRequest(string Name, string Description, bool IsActive);

public record WorkflowDto(
    Guid Id, string Name, string Description, bool IsActive,
    DateTime CreatedAt, List<StepDto> Steps);

// ── Step ──────────────────────────────────────────────
public record CreateStepRequest(
    string Name, string Description, StepType Type,
    int Order, string ConfigJson, List<CreateRuleRequest> Rules);

public record StepDto(
    Guid Id, string Name, string Description, StepType Type,
    int Order, string ConfigJson, List<RuleDto> Rules);

// ── Rule ──────────────────────────────────────────────
public record CreateRuleRequest(string Field, RuleOperator Operator, string Value);
public record RuleDto(Guid Id, string Field, RuleOperator Operator, string Value);

// ── Trigger & Instance ────────────────────────────────
public record TriggerWorkflowRequest(string ContextJson);

public record WorkflowInstanceDto(
    Guid Id, Guid WorkflowId, WorkflowStatus Status,
    int CurrentStepOrder, DateTime StartedAt, DateTime? CompletedAt,
    List<StepLogDto> StepLogs);

public record StepLogDto(
    Guid Id, Guid StepId, string StepName,
    StepStatus Status, string Message, DateTime ExecutedAt);