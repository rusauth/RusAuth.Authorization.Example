namespace RusAuth.Authorization.Example.Services;

using Contracts;

public sealed class ExampleAuthSession
{
    public string? CurrentTransactionId { get; private set; }
    public string? InfoMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void TrackTransaction(string transactionId)
    {
        CurrentTransactionId = transactionId;
        ErrorMessage = null;
    }

    public void SetInfo(string message)
    {
        InfoMessage = message;
        ErrorMessage = null;
    }

    public void SetError(string message)
    {
        ErrorMessage = message;
        InfoMessage = null;
    }

    public void ClearMessages()
    {
        InfoMessage = null;
        ErrorMessage = null;
    }
}

public sealed record ExampleConfirmationFlow
{
    public string TransactionId { get; init; } = string.Empty;
    public string ClientPhoneNumber { get; init; } = string.Empty;
    public string? ConfirmationPhoneNumber { get; init; }
    public CallConfirmationStatus Status { get; init; } = CallConfirmationStatus.Unhandled;
    public string WebHook { get; init; } = string.Empty;
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedOn { get; init; }
    public DateTime? CallbackReceivedOn { get; init; }
    public DateTime? LastManualCheckOn { get; init; }
}