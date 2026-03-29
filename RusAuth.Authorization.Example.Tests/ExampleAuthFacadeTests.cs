namespace RusAuth.Authorization.Example.Tests;

using Contracts;
using Contracts.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services;

public sealed class ExampleAuthFacadeTests
{
    [Fact]
    public async Task StartConfirmationAsync_ShouldTrack_Transaction_And_Store_Flow()
    {
        var client = new FakeRusAuthConfirmationClient
        {
            CallToConfirmResponse = new()
            {
                TransactionId = "txn-001",
                AtcPhoneNumber = "+788005553535"
            }
        };

        var store = new ExampleConfirmationStore();
        var session = new ExampleAuthSession();
        var facade = CreateFacade(client, store, session);

        var flow = await facade.StartConfirmationAsync("+79991234567",
                                                       "https://client.example.com/api/rusauth/callback/confirmation",
                                                       10);

        Assert.Equal("txn-001", session.CurrentTransactionId);
        Assert.Equal("txn-001", flow.TransactionId);
        Assert.Single(store.GetAll());
    }

    [Fact]
    public async Task CheckCurrentConfirmationAsync_ShouldUpdate_Local_Flow_Status()
    {
        var client = new FakeRusAuthConfirmationClient
        {
            CheckConfirmationResponse = CallConfirmationStatus.Success
        };

        var store = new ExampleConfirmationStore();
        var session = new ExampleAuthSession();
        var facade = CreateFacade(client, store, session);

        await facade.StartConfirmationAsync("+79991234567",
                                            "https://client.example.com/api/rusauth/callback/confirmation",
                                            10);

        var flow = await facade.CheckCurrentConfirmationAsync("+79991234567");

        Assert.Equal(CallConfirmationStatus.Success, flow.Status);
        Assert.NotNull(flow.LastManualCheckOn);
        Assert.Contains("подтвердил звонок", session.InfoMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildCallbackUrl_ShouldPoint_To_Local_Callback_Endpoint()
    {
        var facade = CreateFacade(new(),
                                  new(),
                                  new());

        var callbackUrl = facade.BuildCallbackUrl(new("https://client.example.com/demo/"));

        Assert.Equal("https://client.example.com/api/rusauth/callback/confirmation", callbackUrl);
    }

    [Fact]
    public void GetCallbackBearerTokenPreview_ShouldReturn_Configured_Value()
    {
        var facade = CreateFacade(new(),
                                  new(),
                                  new(),
                                  "configured-callback-token");

        Assert.Equal("configured-callback-token", facade.GetCallbackBearerTokenPreview());
    }

    private static ExampleAuthFacade CreateFacade(FakeRusAuthConfirmationClient client,
                                                  ExampleConfirmationStore store,
                                                  ExampleAuthSession session,
                                                  string callbackBearerToken = "example-callback-token") =>
        new(client,
            store,
            session,
            Options.Create(new ExampleCallbackOptions
            {
                CallbackBearerToken = callbackBearerToken
            }),
            NullLogger<ExampleAuthFacade>.Instance);

    private sealed class FakeRusAuthConfirmationClient : IRusAuthConfirmationClient
    {
        public RusAuthCallToConfirmResponse CallToConfirmResponse { get; init; } = new()
        {
            TransactionId = "txn-default",
            AtcPhoneNumber = "88005550000"
        };

        public CallConfirmationStatus CheckConfirmationResponse { get; init; } = CallConfirmationStatus.Unhandled;

        public Task<RusAuthCallToConfirmResponse> CallToConfirmAsync(RusAuthCallToConfirmRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(CallToConfirmResponse);

        public Task<RusCheckConfirmationResponse> CheckConfirmationAsync(RusAuthCheckConfirmationRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new RusCheckConfirmationResponse { Status = CheckConfirmationResponse });
    }
}