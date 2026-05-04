using WorkflowLite.Domain.Enums;

namespace WorkflowLite.Domain.Entities;

public class WorkflowInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowId { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
    public int CurrentStepOrder { get; set; } = 0;

    // JSON payload passed at trigger time, used by rules engine
    public string ContextJson { get; set; } = "{}";

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string TriggeredByUserId { get; set; } = string.Empty;

    public Workflow Workflow { get; set; } = null!;
    public ICollection<StepLog> StepLogs { get; set; } = new List<StepLog>();
}