using Azure.Core;
using Azure.Core.Pipeline;

namespace OrchestratorFunction.Services;

/// <summary>
/// Custom HTTP pipeline policy that adds the x-ms-file-request-intent header
/// required for OAuth authentication with Azure Files.
/// </summary>
public class FileRequestIntentPolicy : HttpPipelinePolicy
{
    private const string FileRequestIntentHeader = "x-ms-file-request-intent";
    private const string BackupIntent = "backup";

    public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        AddFileRequestIntentHeader(message);
        ProcessNext(message, pipeline);
    }

    public override async ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        AddFileRequestIntentHeader(message);
        await ProcessNextAsync(message, pipeline).ConfigureAwait(false);
    }

    private static void AddFileRequestIntentHeader(HttpMessage message)
    {
        if (!message.Request.Headers.Contains(FileRequestIntentHeader))
        {
            message.Request.Headers.Add(FileRequestIntentHeader, BackupIntent);
        }
    }
}
