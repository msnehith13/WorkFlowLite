using WorkflowLite.Domain.Entities;

namespace WorkflowLite.Domain.Interfaces;

public interface IWorkflowRepository
{
    Task<IEnumerable<Workflow>> GetAllAsync(string userId);
    Task<Workflow?> GetByIdAsync(Guid id);
    Task<Workflow> CreateAsync(Workflow workflow);
    Task<Workflow> UpdateAsync(Workflow workflow);
    Task DeleteAsync(Guid id);
}

public interface IWorkflowInstanceRepository
{
    Task<IEnumerable<WorkflowInstance>> GetByWorkflowIdAsync(Guid workflowId);
    Task<WorkflowInstance?> GetByIdAsync(Guid id);
    Task<WorkflowInstance> CreateAsync(WorkflowInstance instance);
    Task<WorkflowInstance> UpdateAsync(WorkflowInstance instance);
}

public interface IStepLogRepository
{
    Task<IEnumerable<StepLog>> GetByInstanceIdAsync(Guid instanceId);
    Task<StepLog> CreateAsync(StepLog log);
}

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser> CreateAsync(AppUser user);
}