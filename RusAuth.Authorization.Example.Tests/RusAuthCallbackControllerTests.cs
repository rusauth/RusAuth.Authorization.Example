namespace RusAuth.Authorization.Example.Tests;

using Contracts;
using Controllers;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services;

public sealed class RusAuthCallbackControllerTests
{
    [Fact]
    public void Authorization_Filter_Should_Reject_Missing_Bearer_Token()
    {
        var filter = CreateAuthorizationFilter("callback-token");
        var context = CreateAuthorizationContext();

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public void Authorization_Filter_Should_Reject_Invalid_Bearer_Token()
    {
        var filter = CreateAuthorizationFilter("callback-token");
        var context = CreateAuthorizationContext("Bearer wrong-token");

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public void Authorization_Filter_Should_Accept_Configured_Bearer_Token()
    {
        var filter = CreateAuthorizationFilter("callback-token");
        var context = CreateAuthorizationContext("Bearer callback-token");

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task Confirmation_Should_Update_Store_And_Publish_Notifier_Event()
    {
        var store = new ExampleConfirmationStore();
        store.Start(new()
        {
            TransactionId = "tx-001",
            AtcPhoneNumber = "+788005550001"
        }, new()
        {
            PhoneNumber = "+79991112233",
            ExpirationMinute = 10,
            WebHook = "https://client.example.com/api/rusauth/callback/confirmation",
            WebHookBearerToken = "callback-token"
        });

        var notifier = new RecordingConfirmationNotifier();
        var controller = new RusAuthCallbackController(store,
                                                       notifier,
                                                       NullLogger<RusAuthCallbackController>.Instance)
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.Confirmation(new()
        {
            TransactionId = "tx-001",
            ClientPhoneNumber = "+79991112233"
        });

        Assert.IsType<OkResult>(result);

        var flow = store.Get("tx-001");
        Assert.NotNull(flow);
        Assert.Equal(CallConfirmationStatus.Success, flow.Status);

        var payload = Assert.IsType<ExampleConfirmationUpdate>(Assert.Single(notifier.PublishedUpdates));
        Assert.Equal("tx-001", payload.TransactionId);
        Assert.Equal(CallConfirmationStatus.Success, payload.Status);
    }

    private static CallbackBearerAuthorizationFilter CreateAuthorizationFilter(string callbackBearerToken) =>
        new(Options.Create(new ExampleCallbackOptions
        {
            CallbackBearerToken = callbackBearerToken
        }));

    private static AuthorizationFilterContext CreateAuthorizationContext(string? authorizationHeader = null)
    {
        var httpContext = new DefaultHttpContext();
        if (string.IsNullOrWhiteSpace(authorizationHeader) is false)
            httpContext.Request.Headers.Authorization = authorizationHeader;

        var actionContext = new ActionContext(httpContext, new(), new());
        return new(actionContext, []);
    }

    private sealed class RecordingConfirmationNotifier : IExampleConfirmationNotifier
    {
        public List<ExampleConfirmationUpdate> PublishedUpdates { get; } = [];

        public event Func<ExampleConfirmationUpdate, Task>? ConfirmationUpdated
        {
            add {}
            remove {}
        }

        public Task PublishAsync(ExampleConfirmationUpdate update, CancellationToken cancellationToken = default)
        {
            PublishedUpdates.Add(update);
            return Task.CompletedTask;
        }
    }
}