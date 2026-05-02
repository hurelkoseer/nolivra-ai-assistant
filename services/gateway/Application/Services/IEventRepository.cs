using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Services;

public interface IEventRepository
{
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CalendarEvent>> GetAllAsync(int page, int pageSize, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    Task DeleteAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
}