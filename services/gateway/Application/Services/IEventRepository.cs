using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Services;

public interface IEventRepository
{
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
