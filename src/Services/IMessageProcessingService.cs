using Azure.Storage.Queues.Models;

namespace OrchestratorFunction.Services;

public interface IMessageProcessingService
{
    Task ProcessMessageAsync(QueueMessage queueMessage, CancellationToken cancellationToken);
}
