namespace RusAuth.Authorization.Example.Services;

using Contracts.Rest;

public sealed record ExampleConfirmationUpdate
{
    public string TransactionId { get; init; } = string.Empty;
    public RusAuthConfirmationStatus Status { get; init; }
    public DateTime? CallbackReceivedOn { get; init; }
    public DateTime? CompletedOn { get; init; }
}