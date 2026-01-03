# Orchestrator Container App

## Purpose

This repository demonstrates how to deploy an Azure Function with a Queue Trigger as a containerized application in Azure Container Apps environment. The function connects to Azure Storage Queue using **Managed Identity** authentication instead of traditional connection strings.

## Key Features

- **Azure Function Queue Trigger**: Processes messages from Azure Storage Queue
- **Containerized Deployment**: Packaged as a Docker container for deployment
- **Azure Container Apps**: Hosted in Azure Container Apps environment
- **Managed Identity Authentication**: Secure, passwordless connection to Azure Storage Queue without storing connection strings

## Architecture

The solution eliminates the need for connection strings by leveraging Azure Managed Identity, providing a more secure approach to accessing Azure Storage resources. This pattern is ideal for production environments where credential management and security are critical concerns.

## Minimal Setup

To configure the Azure Function to use Managed Identity for Storage Queue access:

1. **Enable System-Assigned Managed Identity** on the **Container App** level (NOT on the Container App Environment level)

2. **Assign the following RBAC roles** to the Managed Identity on the Azure Storage Account:
   - `Storage Queue Data Reader`
   - `Storage Queue Data Message Processor`

3. **Add the following environment variables** to the container:
   - `StorageQueue__queueServiceUri` - The URI of the queue service (e.g., `https://<storage-account>.queue.core.windows.net`)
   - `StorageQueue__accountName` - The name of the storage account

## Useful Links

- [Identity-based Connections for Queue Trigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp#identity-based-connections)
- [Edit the AzureWebJobsStorage Configuration](https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial#edit-the-azurewebjobsstorage-configuration)
