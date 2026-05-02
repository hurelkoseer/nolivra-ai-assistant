using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Commands;

public sealed class CreateEventCommandHandler
{
    private readonly IEventRepository _eventRepository;

    public CreateEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> HandleAsync(CreateEventCommand command, CancellationToken cancellationToken = default)
    {
        var calendarEvent = CalendarEvent.Create(command.Title, command.Details, command.StartAtUtc, command.EndAtUtc);
        await _eventRepository.AddAsync(calendarEvent, cancellationToken);
        return calendarEvent.Id;
    }
}
