using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Infrastructure.Persistence;

namespace Nolivra.Gateway.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AssistantDbContext _dbContext;

    public TaskRepository(AssistantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tasks.AddAsync(task, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<TaskItem>> GetAllAsync(
     int page,
     int pageSize,
     string? status = null,
     DateTimeOffset? dueDate = null,
     CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tasks
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (dueDate.HasValue)
        {
            var start = new DateTimeOffset(
    dueDate.Value.Year,
    dueDate.Value.Month,
    dueDate.Value.Day,
    0,
    0,
    0,
    TimeSpan.Zero);

            var end = start.AddDays(1);

            query = query.Where(x => x.DueAt >= start && x.DueAt < end);
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
