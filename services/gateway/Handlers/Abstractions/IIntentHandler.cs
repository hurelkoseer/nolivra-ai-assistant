using Nolivra.Gateway.Models;

namespace Nolivra.Gateway.Handlers.Abstractions;

public interface IIntentHandler
{
    string SupportedIntent { get; }
    Task<HandleIntentResult> HandleAsync(AssistantIntentResult intent, CancellationToken cancellationToken = default);
}