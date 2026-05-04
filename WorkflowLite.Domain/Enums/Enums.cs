namespace WorkflowLite.Domain.Enums;

public enum StepType
{
    AssignToUser = 1,
    SendEmail = 2,
    MarkComplete = 3,
    Approval = 4
}

public enum WorkflowStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum StepStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Skipped = 5
}

public enum RuleOperator
{
    Equals = 1,
    NotEquals = 2,
    GreaterThan = 3,
    LessThan = 4,
    Contains = 5
}