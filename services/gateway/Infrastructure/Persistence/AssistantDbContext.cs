using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Infrastructure.Persistence;

public sealed class AssistantDbContext : DbContext
{
    public AssistantDbContext(DbContextOptions<AssistantDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssistantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}