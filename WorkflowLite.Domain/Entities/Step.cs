using WorkflowLite.Domain.Enums;

namespace WorkflowLite.Domain.Entities;

public class Step
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepType Type { get; set; }
    public int Order { get; set; }

    // JSON-serialized config: e.g. { "to": "admin@co.com", "subject": "Review needed" }
    public string ConfigJson { get; set; } = "{}";

    public Workflow Workflow { get; set; } = null!;
    public ICollection<Rule> Rules { get; set; } = new List<Rule>();
    public ICollection<StepLog> StepLogs { get; set; } = new List<StepLog>();
}