using Microsoft.EntityFrameworkCore;
using WorkflowLite.Domain.Entities;

namespace WorkflowLite.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<Step> Steps => Set<Step>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<StepLog> StepLogs => Set<StepLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workflow>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Name).IsRequired().HasMaxLength(200);
            e.HasMany(w => w.Steps).WithOne(s => s.Workflow).HasForeignKey(s => s.WorkflowId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(w => w.Instances).WithOne(i => i.Workflow).HasForeignKey(i => i.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Step>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.HasMany(s => s.Rules).WithOne(r => r.Step).HasForeignKey(r => r.StepId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rule>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Field).IsRequired().HasMaxLength(100);
            e.Property(r => r.Value).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<WorkflowInstance>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasMany(i => i.StepLogs).WithOne(l => l.WorkflowInstance).HasForeignKey(l => l.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StepLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Step).WithMany(s => s.StepLogs).HasForeignKey(l => l.StepId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(300);
        });
    }
}