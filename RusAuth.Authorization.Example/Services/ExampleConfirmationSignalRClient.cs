namespace RusAuth.Authorization.Example.Services;

using Hubs;
using Microsoft.AspNetCore.SignalR.Client;

public interface IExampleConfirmationSignalRClient : IAsyncDisposable
{
    event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated;
    Task EnsureConnectedAsync(Uri applicationBaseUri, CancellationToken cancellationToken = default);
}

public sealed class ExampleConfirmationSignalRClient(ILogger<ExampleConfirmationSignalRClient> logger) : IExampleConfirmationSignalRClient
{
    private readonly SemaphoreSlim _sync = new(1, 1);
    private HubConnection? _connection;
    private bool _isSubscribed;

    public event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated;

    public async Task EnsureConnectedAsync(Uri applicationBaseUri, CancellationToken cancellationToken = default)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            _connection ??= CreateConnection(applicationBaseUri);

            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync(cancellationToken);

            if (_isSubscribed)
                return;

            await _connection.InvokeAsync(nameof(ExampleConfirmationHub.SubscribeToUpdatesAsync), cancellationToken);
            _isSubscribed = true;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sync.WaitAsync();
        try
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
        finally
        {
            _sync.Release();
            _sync.Dispose();
        }
    }

    private HubConnection CreateConnection(Uri applicationBaseUri)
    {
        var hubUri = new Uri(applicationBaseUri, "/hubs/example-confirmations");

        var connection = new HubConnectionBuilder()
                         .WithUrl(hubUri)
                         .WithAutomaticReconnect()
                         .Build();

        connection.On<ExampleConfirmationUpdate>(ExampleConfirmationHub.ConfirmationUpdatedMethod,
                                                 HandleConfirmationUpdatedAsync);
        connection.Reconnected += async _ => {
            _isSubscribed = false;
            await EnsureConnectedAsync(applicationBaseUri);
        };
        connection.Closed += error => {
            if (error is not null)
                logger.LogWarning(error, "Example confirmation SignalR connection closed.");

            _isSubscribed = false;
            return Task.CompletedTask;
        };

        return connection;
    }

    private async Task HandleConfirmationUpdatedAsync(ExampleConfirmationUpdate update)
    {
        var handlers = ConfirmationUpdated;
        if (handlers is null)
            return;

        foreach (var handler in handlers.GetInvocationList().Cast<Func<ExampleConfirmationUpdate, Task>>())
            await handler(update);
    }
}