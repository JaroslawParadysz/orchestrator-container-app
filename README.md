# Orchestrator Container App

## Purpose

This repository demonstrates how to deploy an Azure Function with a Queue Trigger as a containerized application in Azure Container Apps environment. The function connects to Azure Storage Queue using **Managed Identity** authentication instead of traditional connection strings.

## Key Features

- **Azure Function Queue Trigger**: Processes messages from Azure Storage Queue
- **Blob Storage Integration**: Saves processed message data as JSON files to Azure Blob Storage
- **File Share Integration**: Reads file content from Azure File Share and includes it in the processed data
- **Containerized Deployment**: Packaged as a Docker container for deployment
- **Azure Container Apps**: Hosted in Azure Container Apps environment
- **Managed Identity Authentication**: Secure, passwordless connection to Azure Storage (Queue, Blob, and File Share) without storing connection strings or access keys

## Architecture

The solution eliminates the need for connection strings by leveraging Azure Managed Identity, providing a more secure approach to accessing Azure Storage resources. This pattern is ideal for production environments where credential management and security are critical concerns.

## Minimal Setup

To configure the Azure Function to use Managed Identity for Storage Queue access:

1. **Enable System-Assigned Managed Identity** on the **Container App** level (NOT on the Container App Environment level)

2. **Assign the following RBAC roles** to the Managed Identity on the Azure Storage Account:
   - `Storage Queue Data Reader`
   - `Storage Queue Data Message Processor`
   - `Storage Blob Data Contributor`
   - `Storage File Data Privileged Contributor`

3. **Add the following environment variables** to the container:
   - `StorageQueue__queueServiceUri` - The URI of the queue service (e.g., `https://<storage-account>.queue.core.windows.net`)
   - `StorageQueue__accountName` - The name of the storage account
   - `StorageBlob__blobServiceUri` - The URI of the blob service (e.g., `https://<storage-account>.blob.core.windows.net`)
   - `StorageBlob__containerName` - The blob container name (defaults to `processed-messages`)
   - `StorageFile__fileServiceUri` - The URI of the file service (e.g., `https://<storage-account>.file.core.windows.net`)
   - `StorageFile__shareName` - The name of the file share

## Useful Links

- [Identity-based Connections for Queue Trigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp#identity-based-connections)
- [Edit the AzureWebJobsStorage Configuration](https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial#edit-the-azurewebjobsstorage-configuration)
