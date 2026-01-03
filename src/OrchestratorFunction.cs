using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrchestratorFunction;

public class OrchestratorFunction
{
    private readonly ILogger<OrchestratorFunction> _logger;
    private readonly string _blobServiceUri;
    private readonly string _containerName;

    public OrchestratorFunction(ILogger<OrchestratorFunction> logger)
    {
        _logger = logger;
        _blobServiceUri = Environment.GetEnvironmentVariable("StorageBlob__blobServiceUri") 
                          ?? throw new InvalidOperationException("StorageBlob__blobServiceUri environment variable is not set");
        _containerName = Environment.GetEnvironmentVariable("StorageBlob__containerName") ?? "processed-messages";
    }

    [Function(nameof(OrchestratorFunction))]
    public async Task Run(
        [QueueTrigger("orchestrator-dev", Connection = "StorageQueue")] QueueMessage queueMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queue trigger function processed message: {Message}", queueMessage);
        
        try
        {
            // Create a blob client using managed identity
            var blobServiceClient = new BlobServiceClient(new Uri(_blobServiceUri), new DefaultAzureCredential());
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            
            // Create JSON data from queue message
            var data = new
            {
                MessageId = queueMessage.MessageId,
                InsertedOn = queueMessage.InsertedOn,
                ProcessedAt = DateTime.UtcNow,
                MessageText = queueMessage.MessageText,
                DequeueCount = queueMessage.DequeueCount
            };
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var blobName = $"message-{queueMessage.MessageId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            
            // Upload to blob storage
            var blobClient = containerClient.GetBlobClient(blobName);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Successfully uploaded message to blob storage: {BlobName}", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queue message and uploading to blob storage");
            throw;
        }
    }
}
