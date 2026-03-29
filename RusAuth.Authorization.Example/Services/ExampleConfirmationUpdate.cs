namespace RusAuth.Authorization.Example.Services;

using Contracts;

public sealed record ExampleConfirmationUpdate
{
    public string TransactionId { get; init; } = string.Empty;
    public CallConfirmationStatus Status { get; init; }
    public DateTime? CallbackReceivedOn { get; init; }
    public DateTime? CompletedOn { get; init; }
}