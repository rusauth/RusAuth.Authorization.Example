namespace RusAuth.Authorization.Example.Tests;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using global::RusAuth.Authorization.Contracts.Rest;
using RusAuth.Authorization.Example.Components.Pages;
using RusAuth.Authorization.Example.Services;

public sealed class AuthorizePageTests : TestContext
{
    [Fact]
    public void Authorize_Page_Should_Render_Without_InputBase_Runtime_Error()
    {
        Services.AddSingleton<IRusAuthConfirmationClient>(new FakeRusAuthConfirmationClient());
        Services.AddSingleton<ExampleConfirmationStore>();
        Services.AddSingleton<ExampleAuthSession>();
        Services.AddSingleton<IOptions<ExampleCallbackOptions>>(Options.Create(new ExampleCallbackOptions
        {
            CallbackBearerToken = "test-callback-token"
        }));
        Services.AddSingleton<IExampleConfirmationSignalRClient>(new FakeExampleConfirmationSignalRClient());
        Services.AddSingleton<ExampleAuthFacade>(sp => new ExampleAuthFacade(
            sp.GetRequiredService<IRusAuthConfirmationClient>(),
            sp.GetRequiredService<ExampleConfirmationStore>(),
            sp.GetRequiredService<ExampleAuthSession>(),
            sp.GetRequiredService<IOptions<ExampleCallbackOptions>>(),
            NullLogger<ExampleAuthFacade>.Instance));

        var cut = RenderComponent<Authorize>();

        Assert.Contains("Запуск подтверждения звонком", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("SignalR", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Вебхук", cut.Markup, StringComparison.Ordinal);
        Assert.NotNull(cut.Find("#callback").GetAttribute("readonly"));
    }

    private sealed class FakeRusAuthConfirmationClient : IRusAuthConfirmationClient
    {
        public Task<RusAuthCallToConfirmResponse> CallToConfirmAsync(RusAuthCallToConfirmRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new RusAuthCallToConfirmResponse
            {
                TransactionId = "txn-test",
                AtcPhoneNumber = new RusAuthPhoneNumber
                {
                    CountryCode = 7,
                    Number = 88005550000
                }
            });

        public Task<RusAuthConfirmationStatus> CheckConfirmationAsync(RusAuthCheckConfirmationRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(RusAuthConfirmationStatus.Unhandled);
    }

    private sealed class FakeExampleConfirmationSignalRClient : IExampleConfirmationSignalRClient
    {
        public event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated
        {
            add {}
            remove {}
        }

        public Task EnsureConnectedAsync(Uri applicationBaseUri, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
