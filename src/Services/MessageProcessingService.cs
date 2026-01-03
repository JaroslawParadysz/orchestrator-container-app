using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;

namespace OrchestratorFunction.Services;

public class MessageProcessingService : IMessageProcessingService
{
    private readonly ILogger<MessageProcessingService> _logger;
    private readonly string _blobServiceUri;
    private readonly string _containerName;
    private readonly string _fileServiceUri;
    private readonly string _fileShareName;

    public MessageProcessingService(ILogger<MessageProcessingService> logger)
    {
        _logger = logger;
        _blobServiceUri = Environment.GetEnvironmentVariable("StorageBlob__blobServiceUri") 
                          ?? throw new InvalidOperationException("StorageBlob__blobServiceUri environment variable is not set");
        _containerName = Environment.GetEnvironmentVariable("StorageBlob__containerName") ?? "processed-messages";
        _fileServiceUri = Environment.GetEnvironmentVariable("StorageFile__fileServiceUri")
                          ?? throw new InvalidOperationException("StorageFile__fileServiceUri environment variable is not set");
        _fileShareName = Environment.GetEnvironmentVariable("StorageFile__shareName")
                         ?? throw new InvalidOperationException("StorageFile__shareName environment variable is not set");
    }

    public async Task ProcessMessageAsync(QueueMessage queueMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing message: {MessageId}", queueMessage.MessageId);
        
        // Read file content from file share using managed identity
        // Using custom HTTP pipeline policy to add required x-ms-file-request-intent header
        string? fileShareContent = null;
        try
        {
            var shareClientOptions = new ShareClientOptions();
            shareClientOptions.AddPolicy(new FileRequestIntentPolicy(), HttpPipelinePosition.PerCall);
            
            var fileServiceClient = new ShareServiceClient(new Uri(_fileServiceUri), new DefaultAzureCredential(), shareClientOptions);
            var shareClient = fileServiceClient.GetShareClient(_fileShareName);
            var directoryClient = shareClient.GetDirectoryClient("files");
            var fileClient = directoryClient.GetFileClient("file.txt");
            
            var downloadInfo = await fileClient.DownloadAsync(cancellationToken: cancellationToken);
            using var reader = new StreamReader(downloadInfo.Value.Content);
            fileShareContent = await reader.ReadToEndAsync(cancellationToken);
            
            _logger.LogInformation("Successfully read file from file share using managed identity");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file from file share, continuing without it");
        }
        
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
            DequeueCount = queueMessage.DequeueCount,
            FileShareContent = fileShareContent
        };
        
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var blobName = $"message-{queueMessage.MessageId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        
        // Upload to blob storage
        var blobClient = containerClient.GetBlobClient(blobName);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        
        _logger.LogInformation("Successfully uploaded message to blob storage: {BlobName}", blobName);
    }
}
