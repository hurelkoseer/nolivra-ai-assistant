using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Commands;

public sealed class CreateTaskCommandHandler
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Guid> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken = default)
    {
        var task = TaskItem.Create(command.Title, command.Details, command.DueAt);
        await _taskRepository.AddAsync(task, cancellationToken);
        return task.Id;
    }
}