namespace RusAuth.Authorization.Example.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using global::RusAuth.Authorization.Contracts.Rest;
using RusAuth.Authorization.Example.Services;

public sealed class ExampleAuthFacadeTests
{
    [Fact]
    public async Task StartConfirmationAsync_ShouldTrack_Transaction_And_Store_Flow()
    {
        var client = new FakeRusAuthConfirmationClient
        {
            CallToConfirmResponse = new RusAuthCallToConfirmResponse
            {
                TransactionId = "txn-001",
                AtcPhoneNumber = new RusAuthPhoneNumber { CountryCode = 7, Number = 88005553535 }
            }
        };

        var store = new ExampleConfirmationStore();
        var session = new ExampleAuthSession();
        var facade = CreateFacade(client, store, session);

        var flow = await facade.StartConfirmationAsync(new RusAuthPhoneNumber { CountryCode = 7, Number = 9991234567 },
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
            CheckConfirmationResponse = RusAuthConfirmationStatus.Success
        };

        var store = new ExampleConfirmationStore();
        var session = new ExampleAuthSession();
        var facade = CreateFacade(client, store, session);

        await facade.StartConfirmationAsync(new RusAuthPhoneNumber { CountryCode = 7, Number = 9991234567 },
                                            "https://client.example.com/api/rusauth/callback/confirmation",
                                            10);

        var flow = await facade.CheckCurrentConfirmationAsync(new RusAuthPhoneNumber { CountryCode = 7, Number = 9991234567 });

        Assert.Equal(RusAuthConfirmationStatus.Success, flow.Status);
        Assert.NotNull(flow.LastManualCheckOn);
        Assert.Contains("подтвердил звонок", session.InfoMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildCallbackUrl_ShouldPoint_To_Local_Callback_Endpoint()
    {
        var facade = CreateFacade(new FakeRusAuthConfirmationClient(),
                                  new ExampleConfirmationStore(),
                                  new ExampleAuthSession());

        var callbackUrl = facade.BuildCallbackUrl(new Uri("https://client.example.com/demo/"));

        Assert.Equal("https://client.example.com/api/rusauth/callback/confirmation", callbackUrl);
    }

    [Fact]
    public void GetCallbackBearerTokenPreview_ShouldReturn_Configured_Value()
    {
        var facade = CreateFacade(new FakeRusAuthConfirmationClient(),
                                  new ExampleConfirmationStore(),
                                  new ExampleAuthSession(),
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
            AtcPhoneNumber = new RusAuthPhoneNumber { CountryCode = 7, Number = 88005550000 }
        };

        public RusAuthConfirmationStatus CheckConfirmationResponse { get; init; } = RusAuthConfirmationStatus.Unhandled;

        public Task<RusAuthCallToConfirmResponse> CallToConfirmAsync(RusAuthCallToConfirmRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(CallToConfirmResponse);

        public Task<RusAuthConfirmationStatus> CheckConfirmationAsync(RusAuthCheckConfirmationRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(CheckConfirmationResponse);
    }
}
