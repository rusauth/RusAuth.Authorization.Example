namespace RusAuth.Authorization.Example.Services;

using System.Collections.Concurrent;
using Contracts;
using Contracts.Rest;

public sealed class ExampleConfirmationStore
{
    private readonly ConcurrentDictionary<string, ExampleConfirmationFlow> _flows = new(StringComparer.Ordinal);

    public ExampleConfirmationFlow Start(RusAuthCallToConfirmResponse response, RusAuthCallToConfirmRequest request)
    {
        var flow = new ExampleConfirmationFlow
        {
            TransactionId = response.TransactionId,
            ClientPhoneNumber = request.PhoneNumber,
            ConfirmationPhoneNumber = response.AtcPhoneNumber,
            Status = CallConfirmationStatus.Unhandled,
            WebHook = request.WebHook,
            CreatedOn = DateTime.UtcNow
        };

        _flows[flow.TransactionId] = flow;
        return flow;
    }

    public ExampleConfirmationFlow? Get(string transactionId) =>
        _flows.GetValueOrDefault(transactionId);

    public IReadOnlyCollection<ExampleConfirmationFlow> GetAll() =>
        _flows.Values.OrderByDescending(static x => x.CreatedOn).ToArray();

    public ExampleConfirmationFlow? MarkCallbackReceived(RusAuthConfirmationWebHookRequest request)
    {
        if (!_flows.TryGetValue(request.TransactionId, out var existing))
            return null;

        var updated = existing with
        {
            Status = CallConfirmationStatus.Success,
            CompletedOn = DateTime.UtcNow,
            CallbackReceivedOn = DateTime.UtcNow
        };

        _flows[request.TransactionId] = updated;
        return updated;
    }

    public ExampleConfirmationFlow? MarkManualStatus(string transactionId, CallConfirmationStatus status)
    {
        if (!_flows.TryGetValue(transactionId, out var existing))
            return null;

        var completedOn = status == CallConfirmationStatus.Unhandled ? existing.CompletedOn : DateTime.UtcNow;
        var updated = existing with
        {
            Status = status,
            CompletedOn = completedOn,
            LastManualCheckOn = DateTime.UtcNow
        };

        _flows[transactionId] = updated;
        return updated;
    }
}