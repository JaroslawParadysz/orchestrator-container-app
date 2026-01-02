using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrchestratorFunction;

public class OrchestratorFunction
{
    private readonly ILogger<OrchestratorFunction> _logger;

    public OrchestratorFunction(ILogger<OrchestratorFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OrchestratorFunction))]
    public async Task Run(
        [QueueTrigger("orchestrator-dev", Connection = "StorageQueue")] QueueMessage queueMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queue trigger function processed message: {Message}", queueMessage);
        
        // Add your orchestration logic here
        await Task.CompletedTask;
    }
}
