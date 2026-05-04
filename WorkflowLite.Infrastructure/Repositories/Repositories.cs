using Microsoft.EntityFrameworkCore;
using WorkflowLite.Domain.Entities;
using WorkflowLite.Domain.Interfaces;
using WorkflowLite.Infrastructure.Data;

namespace WorkflowLite.Infrastructure.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly AppDbContext _db;
    public WorkflowRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Workflow>> GetAllAsync(string userId) =>
        await _db.Workflows
            .Include(w => w.Steps).ThenInclude(s => s.Rules)
            .Where(w => w.CreatedByUserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

    public async Task<Workflow?> GetByIdAsync(Guid id) =>
        await _db.Workflows
            .Include(w => w.Steps.OrderBy(s => s.Order)).ThenInclude(s => s.Rules)
            .FirstOrDefaultAsync(w => w.Id == id);

    public async Task<Workflow> CreateAsync(Workflow workflow)
    {
        _db.Workflows.Add(workflow);
        await _db.SaveChangesAsync();
        return workflow;
    }

    public async Task<Workflow> UpdateAsync(Workflow workflow)
    {
        workflow.UpdatedAt = DateTime.UtcNow;
        _db.Workflows.Update(workflow);
        await _db.SaveChangesAsync();
        return workflow;
    }

    public async Task DeleteAsync(Guid id)
    {
        var workflow = await _db.Workflows.FindAsync(id);
        if (workflow != null)
        {
            _db.Workflows.Remove(workflow);
            await _db.SaveChangesAsync();
        }
    }
}

public class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly AppDbContext _db;
    public WorkflowInstanceRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<WorkflowInstance>> GetByWorkflowIdAsync(Guid workflowId) =>
        await _db.WorkflowInstances
            .Include(i => i.StepLogs)
            .Where(i => i.WorkflowId == workflowId)
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync();

    public async Task<WorkflowInstance?> GetByIdAsync(Guid id) =>
        await _db.WorkflowInstances
            .Include(i => i.StepLogs).ThenInclude(l => l.Step)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<WorkflowInstance> CreateAsync(WorkflowInstance instance)
    {
        _db.WorkflowInstances.Add(instance);
        await _db.SaveChangesAsync();
        return instance;
    }

    public async Task<WorkflowInstance> UpdateAsync(WorkflowInstance instance)
    {
        _db.WorkflowInstances.Update(instance);
        await _db.SaveChangesAsync();
        return instance;
    }
}

public class StepLogRepository : IStepLogRepository
{
    private readonly AppDbContext _db;
    public StepLogRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<StepLog>> GetByInstanceIdAsync(Guid instanceId) =>
        await _db.StepLogs
            .Include(l => l.Step)
            .Where(l => l.WorkflowInstanceId == instanceId)
            .OrderBy(l => l.ExecutedAt)
            .ToListAsync();

    public async Task<StepLog> CreateAsync(StepLog log)
    {
        _db.StepLogs.Add(log);
        await _db.SaveChangesAsync();
        return log;
    }
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<AppUser?> GetByEmailAsync(string email) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<AppUser?> GetByIdAsync(string id) =>
        await _db.Users.FindAsync(id);

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        user.Email = user.Email.ToLower();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}