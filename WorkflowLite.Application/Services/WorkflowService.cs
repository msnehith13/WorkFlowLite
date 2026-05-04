using System.Text.Json;
using WorkflowLite.Application.DTOs;
using WorkflowLite.Domain.Entities;
using WorkflowLite.Domain.Enums;
using WorkflowLite.Domain.Interfaces;

namespace WorkflowLite.Application.Services;

public class WorkflowService
{
    private readonly IWorkflowRepository _workflows;
    private readonly IWorkflowInstanceRepository _instances;
    private readonly IStepLogRepository _stepLogs;

    public WorkflowService(
        IWorkflowRepository workflows,
        IWorkflowInstanceRepository instances,
        IStepLogRepository stepLogs)
    {
        _workflows = workflows;
        _instances = instances;
        _stepLogs = stepLogs;
    }

    public async Task<IEnumerable<WorkflowDto>> GetAllAsync(string userId)
    {
        var workflows = await _workflows.GetAllAsync(userId);
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto> GetByIdAsync(Guid id)
    {
        var workflow = await _workflows.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Workflow {id} not found.");
        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> CreateAsync(CreateWorkflowRequest req, string userId)
    {
        var workflow = new Workflow
        {
            Name = req.Name,
            Description = req.Description,
            CreatedByUserId = userId,
            Steps = req.Steps.Select(s => new Step
            {
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                Order = s.Order,
                ConfigJson = s.ConfigJson,
                Rules = s.Rules.Select(r => new Rule
                {
                    Field = r.Field,
                    Operator = r.Operator,
                    Value = r.Value
                }).ToList()
            }).ToList()
        };

        var created = await _workflows.CreateAsync(workflow);
        return MapToDto(created);
    }

    public async Task<WorkflowDto> UpdateAsync(Guid id, UpdateWorkflowRequest req)
    {
        var workflow = await _workflows.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Workflow {id} not found.");

        workflow.Name = req.Name;
        workflow.Description = req.Description;
        workflow.IsActive = req.IsActive;

        var updated = await _workflows.UpdateAsync(workflow);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _workflows.DeleteAsync(id);
    }

    // ── Trigger ──────────────────────────────────────────────────────────────

    public async Task<WorkflowInstanceDto> TriggerAsync(Guid workflowId, string userId, TriggerWorkflowRequest req)
    {
        var workflow = await _workflows.GetByIdAsync(workflowId)
            ?? throw new KeyNotFoundException($"Workflow {workflowId} not found.");

        if (!workflow.IsActive)
            throw new InvalidOperationException("Cannot trigger an inactive workflow.");

        var instance = new WorkflowInstance
        {
            WorkflowId = workflowId,
            Status = WorkflowStatus.Running,
            ContextJson = req.ContextJson,
            TriggeredByUserId = userId
        };

        await _instances.CreateAsync(instance);

        var context = JsonSerializer.Deserialize<Dictionary<string, string>>(req.ContextJson)
                      ?? new Dictionary<string, string>();

        var orderedSteps = workflow.Steps.OrderBy(s => s.Order).ToList();

        foreach (var step in orderedSteps)
        {
            instance.CurrentStepOrder = step.Order;

            // Evaluate rules — all must pass to execute the step
            bool allRulesPassed = step.Rules.All(r => EvaluateRule(r, context));

            var log = new StepLog
            {
                WorkflowInstanceId = instance.Id,
                StepId = step.Id,
                ExecutedAt = DateTime.UtcNow
            };

            if (!allRulesPassed)
            {
                log.Status = StepStatus.Skipped;
                log.Message = "Step skipped: one or more rules not satisfied.";
                await _stepLogs.CreateAsync(log);
                continue;
            }

            try
            {
                // Execute the step action (extensible by StepType)
                await ExecuteStepAsync(step, context);
                log.Status = StepStatus.Completed;
                log.Message = $"Step '{step.Name}' executed successfully.";
            }
            catch (Exception ex)
            {
                log.Status = StepStatus.Failed;
                log.Message = $"Step failed: {ex.Message}";
                await _stepLogs.CreateAsync(log);

                instance.Status = WorkflowStatus.Failed;
                await _instances.UpdateAsync(instance);

                return await BuildInstanceDtoAsync(instance);
            }

            await _stepLogs.CreateAsync(log);
        }

        instance.Status = WorkflowStatus.Completed;
        instance.CompletedAt = DateTime.UtcNow;
        await _instances.UpdateAsync(instance);

        return await BuildInstanceDtoAsync(instance);
    }

    // ── Rules engine ─────────────────────────────────────────────────────────

    private static bool EvaluateRule(Rule rule, Dictionary<string, string> context)
    {
        if (!context.TryGetValue(rule.Field, out var actualValue))
            return false;

        return rule.Operator switch
        {
            RuleOperator.Equals => actualValue.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.NotEquals => !actualValue.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.Contains => actualValue.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.GreaterThan => double.TryParse(actualValue, out var a) &&
                                        double.TryParse(rule.Value, out var b) && a > b,
            RuleOperator.LessThan => double.TryParse(actualValue, out var a2) &&
                                      double.TryParse(rule.Value, out var b2) && a2 < b2,
            _ => false
        };
    }

    // ── Step execution (extensible) ───────────────────────────────────────────

    private static Task ExecuteStepAsync(Step step, Dictionary<string, string> context)
    {
        // Phase 1: log intent only. Phase 2: wire real email via IEmailService injection.
        return step.Type switch
        {
            StepType.AssignToUser => Task.CompletedTask,
            StepType.SendEmail => Task.CompletedTask,   // injected in Phase 2
            StepType.MarkComplete => Task.CompletedTask,
            StepType.Approval => Task.CompletedTask,
            _ => throw new NotSupportedException($"Step type {step.Type} not implemented.")
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<WorkflowInstanceDto> BuildInstanceDtoAsync(WorkflowInstance instance)
    {
        var logs = await _stepLogs.GetByInstanceIdAsync(instance.Id);
        return new WorkflowInstanceDto(
            instance.Id, instance.WorkflowId, instance.Status,
            instance.CurrentStepOrder, instance.StartedAt, instance.CompletedAt,
            logs.Select(l => new StepLogDto(
                l.Id, l.StepId, l.Step?.Name ?? "",
                l.Status, l.Message, l.ExecutedAt)).ToList());
    }

    private static WorkflowDto MapToDto(Workflow w) => new(
        w.Id, w.Name, w.Description, w.IsActive, w.CreatedAt,
        w.Steps.OrderBy(s => s.Order).Select(s => new StepDto(
            s.Id, s.Name, s.Description, s.Type, s.Order, s.ConfigJson,
            s.Rules.Select(r => new RuleDto(r.Id, r.Field, r.Operator, r.Value)).ToList()
        )).ToList());
}