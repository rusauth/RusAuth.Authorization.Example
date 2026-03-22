namespace RusAuth.Authorization.Example.Services;

using Contracts.Rest;
using Microsoft.Extensions.Options;

public sealed class ExampleAuthFacade(
    IRusAuthConfirmationClient rusAuthClient,
    ExampleConfirmationStore store,
    ExampleAuthSession session,
    IOptions<ExampleCallbackOptions> callbackOptions,
    ILogger<ExampleAuthFacade> logger)
{
    private readonly ExampleCallbackOptions _callbackOptions = callbackOptions.Value;

    public async Task<ExampleConfirmationFlow> StartConfirmationAsync(RusAuthPhoneNumber phoneNumber,
                                                                      string callbackUrl,
                                                                      int expirationMinute,
                                                                      CancellationToken cancellationToken = default)
    {
        var request = new RusAuthCallToConfirmRequest
        {
            PhoneNumber = phoneNumber,
            ExpirationMinute = expirationMinute,
            WebHook = callbackUrl,
            WebHookBearerToken = _callbackOptions.CallbackBearerToken
        };

        var response = await rusAuthClient.CallToConfirmAsync(request, cancellationToken);
        var flow = store.Start(response, request);

        session.TrackTransaction(flow.TransactionId);
        session.SetInfo("Запрос на подтверждение отправлен в РосАвт. Ожидайте входящий вызов или выполните ручную проверку статуса.");
        logger.LogInformation("Started external RusAuth confirmation transaction {TransactionId}", flow.TransactionId);

        return flow;
    }

    public async Task<ExampleConfirmationFlow> CheckCurrentConfirmationAsync(RusAuthPhoneNumber phoneNumber,
                                                                             CancellationToken cancellationToken = default)
    {
        var transactionId = session.CurrentTransactionId ??
                            throw new InvalidOperationException("Нет активной транзакции подтверждения.");

        var status = await rusAuthClient.CheckConfirmationAsync(new()
        {
            PhoneNumber = phoneNumber,
            TransactionId = transactionId
        }, cancellationToken);

        var flow = store.MarkManualStatus(transactionId, status) ??
                   throw new InvalidOperationException("Транзакция не найдена в локальном журнале примера.");

        session.SetInfo(status switch
                        {
                            RusAuthConfirmationStatus.Success => "РосАвт подтвердил звонок. В реальном проекте здесь вы применяете свои правила входа.",
                            RusAuthConfirmationStatus.Failed  => "РосАвт сообщил, что подтверждение не найдено.",
                            RusAuthConfirmationStatus.Expired => "РосАвт сообщил, что срок подтверждения истёк.",
                            _                                 => "РосАвт всё ещё ожидает подтверждение."
                        });

        return flow;
    }

    public ExampleConfirmationFlow? GetCurrentFlow()
    {
        var transactionId = session.CurrentTransactionId;
        return string.IsNullOrWhiteSpace(transactionId) ? null : store.Get(transactionId);
    }

    public IReadOnlyCollection<ExampleConfirmationFlow> GetTrackedFlows() => store.GetAll();

    public string GetCallbackBearerTokenPreview() => _callbackOptions.CallbackBearerToken;

    public string BuildCallbackUrl(Uri applicationBaseUri) =>
        new Uri(applicationBaseUri, "/api/rusauth/callback/confirmation").ToString();
}

public sealed class ExampleCallbackOptions
{
    public string CallbackBearerToken { get; set; } = "example-callback-token";
}