using WorkflowLite.Domain.Enums;

namespace WorkflowLite.Domain.Entities;

public class Rule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StepId { get; set; }

    // e.g. "status", "amount", "approvedBy"
    public string Field { get; set; } = string.Empty;

    public RuleOperator Operator { get; set; }

    // e.g. "approved", "1000", "true"
    public string Value { get; set; } = string.Empty;

    public Step Step { get; set; } = null!;
}