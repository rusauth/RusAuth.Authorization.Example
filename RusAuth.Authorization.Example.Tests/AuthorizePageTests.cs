namespace RusAuth.Authorization.Example.Tests;

using Bunit;
using Components.Pages;
using Contracts;
using Contracts.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services;

public sealed class AuthorizePageTests : TestContext
{
    [Fact]
    public void Authorize_Page_Should_Render_Without_InputBase_Runtime_Error()
    {
        Services.AddSingleton<IRusAuthConfirmationClient>(new FakeRusAuthConfirmationClient());
        Services.AddSingleton<ExampleConfirmationStore>();
        Services.AddSingleton<ExampleAuthSession>();
        Services.AddSingleton(Options.Create(new ExampleCallbackOptions
        {
            CallbackBearerToken = "test-callback-token"
        }));
        Services.AddSingleton<IExampleConfirmationNotifier>(new FakeExampleConfirmationNotifier());
        Services.AddSingleton<ExampleAuthFacade>(sp => new(
                                                 sp.GetRequiredService<IRusAuthConfirmationClient>(),
                                                 sp.GetRequiredService<ExampleConfirmationStore>(),
                                                 sp.GetRequiredService<ExampleAuthSession>(),
                                                 sp.GetRequiredService<IOptions<ExampleCallbackOptions>>(),
                                                 NullLogger<ExampleAuthFacade>.Instance));

        var cut = RenderComponent<Authorize>();

        Assert.Contains("Запуск подтверждения звонком", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("автоматически", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Вебхук", cut.Markup, StringComparison.Ordinal);
        Assert.NotNull(cut.Find("#callback").GetAttribute("readonly"));
    }

    private sealed class FakeRusAuthConfirmationClient : IRusAuthConfirmationClient
    {
        public Task<RusAuthCallToConfirmResponse> CallToConfirmAsync(RusAuthCallToConfirmRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new RusAuthCallToConfirmResponse
            {
                TransactionId = "txn-test",
                AtcPhoneNumber = "+79996541234"
            });

        public Task<RusCheckConfirmationResponse> CheckConfirmationAsync(RusAuthCheckConfirmationRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new RusCheckConfirmationResponse { Status = CallConfirmationStatus.Unhandled });
    }

    private sealed class FakeExampleConfirmationNotifier : IExampleConfirmationNotifier
    {
        public event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated
        {
            add {}
            remove {}
        }

        public Task PublishAsync(ExampleConfirmationUpdate update, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}