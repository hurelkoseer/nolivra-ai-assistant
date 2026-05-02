using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Services;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetAllAsync(int page, int pageSize, string? status = null, DateTimeOffset? dueDate = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}