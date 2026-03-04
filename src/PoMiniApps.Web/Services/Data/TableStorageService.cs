using Azure.Data.Tables;

namespace PoMiniApps.Web.Services.Data;

/// <summary>
/// Generic Azure Table Storage service using Repository pattern abstraction.
/// </summary>
public class TableStorageService(IConfiguration configuration, ILogger<TableStorageService> logger) : ITableStorageService
{
    private readonly string _connectionString = configuration["PoMiniApps:AzureStorageConnectionString"]
        ?? configuration["PoMiniApps:Azure:StorageConnectionString"]
        ?? configuration["AzureStorageConnectionString"]
        ?? configuration["Azure:StorageConnectionString"]
        ?? throw new InvalidOperationException("PoMiniApps:AzureStorageConnectionString, PoMiniApps:Azure:StorageConnectionString, AzureStorageConnectionString or Azure:StorageConnectionString not found in configuration.");

    private TableServiceClient GetTableServiceClient() => new TableServiceClient(_connectionString);

    public async Task<TableClient> GetTableClientAsync(string tableName)
    {
        var tableClient = GetTableServiceClient().GetTableClient(tableName);
        try
        {
            await tableClient.CreateIfNotExistsAsync();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            // Azurite may throw 400 on table creation; table likely exists or will be created on first use
            logger.LogWarning(ex, "Table creation returned 400 for {TableName}. Continuing...", tableName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating table {TableName}", tableName);
            throw;
        }
        return tableClient;
    }

    public async Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
    {
        try
        {
            var tableClient = await GetTableClientAsync(tableName);
            return await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting entity from {Table} PK={PK} RK={RK}", tableName, partitionKey, rowKey);
            return null;
        }
    }

    public async IAsyncEnumerable<T> GetEntitiesAsync<T>(string tableName, string? filter = null) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync(tableName);
        await foreach (var entity in tableClient.QueryAsync<T>(filter))
            yield return entity;
    }

    public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync(tableName);
        await tableClient.AddEntityAsync(entity);
    }

    public async Task UpsertEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync(tableName);
        await tableClient.UpsertEntityAsync(entity);
    }

    public async Task DeleteEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync(tableName);
        await tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }
}
