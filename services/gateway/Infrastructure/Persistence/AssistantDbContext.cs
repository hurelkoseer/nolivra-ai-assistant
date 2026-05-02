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
    public DbSet<NoteItem> Notes => Set<NoteItem>();
    public DbSet<CalendarEvent> Events => Set<CalendarEvent>();
    public DbSet<AssistantRequestLog> AssistantRequestLogs => Set<AssistantRequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssistantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}