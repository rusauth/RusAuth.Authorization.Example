namespace RusAuth.Authorization.Example.Services;

public interface IExampleConfirmationNotifier
{
    event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated;
    Task PublishAsync(ExampleConfirmationUpdate update, CancellationToken cancellationToken = default);
}

public sealed class ExampleConfirmationNotifier(ILogger<ExampleConfirmationNotifier> logger) : IExampleConfirmationNotifier
{
    public event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated;

    public async Task PublishAsync(ExampleConfirmationUpdate update, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Publishing confirmation update. TransactionId={TransactionId} Status={Status}",
                              update.TransactionId,
                              update.Status);

        var handlers = ConfirmationUpdated;
        if (handlers is null)
            return;

        foreach (var handler in handlers.GetInvocationList().Cast<Func<ExampleConfirmationUpdate, Task>>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await handler(update);
        }
    }
}
