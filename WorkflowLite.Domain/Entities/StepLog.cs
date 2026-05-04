using WorkflowLite.Domain.Enums;

namespace WorkflowLite.Domain.Entities;

public class StepLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowInstanceId { get; set; }
    public Guid StepId { get; set; }
    public StepStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    public WorkflowInstance WorkflowInstance { get; set; } = null!;
    public Step Step { get; set; } = null!;
}