using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrchestratorFunction.Services;

namespace OrchestratorFunction;

public class OrchestratorFunction
{
    private readonly ILogger<OrchestratorFunction> _logger;
    private readonly IMessageProcessingService _messageProcessingService;

    public OrchestratorFunction(ILogger<OrchestratorFunction> logger, IMessageProcessingService messageProcessingService)
    {
        _logger = logger;
        _messageProcessingService = messageProcessingService;
    }

    [Function(nameof(OrchestratorFunction))]
    public async Task Run(
        [QueueTrigger("orchestrator-dev", Connection = "StorageQueue")] QueueMessage queueMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queue trigger function received message: {Message}", queueMessage);
        
        try
        {
            await _messageProcessingService.ProcessMessageAsync(queueMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queue message");
            throw;
        }
    }
}
