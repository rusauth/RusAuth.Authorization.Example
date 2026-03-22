namespace RusAuth.Authorization.Example.Tests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using global::RusAuth.Authorization.Contracts.Rest;
using RusAuth.Authorization.Example.Controllers;
using RusAuth.Authorization.Example.Hubs;
using RusAuth.Authorization.Example.Infrastructure;
using RusAuth.Authorization.Example.Services;

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
    public async Task Confirmation_Should_Update_Store_And_Publish_SignalR_Event()
    {
        var store = new ExampleConfirmationStore();
        store.Start(new()
        {
            TransactionId = "tx-001",
            AtcPhoneNumber = new RusAuthPhoneNumber { CountryCode = 7, Number = 88005550001 }
        }, new RusAuthCallToConfirmRequest
        {
            PhoneNumber = new RusAuthPhoneNumber { CountryCode = 7, Number = 9991112233 },
            ExpirationMinute = 10,
            WebHook = "https://client.example.com/api/rusauth/callback/confirmation",
            WebHookBearerToken = "callback-token"
        });

        var clientProxy = new RecordingClientProxy();
        var hubContext = new FakeHubContext(clientProxy);
        var controller = new RusAuthCallbackController(store,
                                                       hubContext,
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
            ClientPhoneNumber = new RusAuthPhoneNumber { CountryCode = 7, Number = 9991112233 }
        });

        Assert.IsType<OkResult>(result);

        var flow = store.Get("tx-001");
        Assert.NotNull(flow);
        Assert.Equal(RusAuthConfirmationStatus.Success, flow.Status);
        Assert.Equal(ExampleConfirmationHub.ConfirmationUpdatedMethod, clientProxy.MethodName);

        var payload = Assert.IsType<ExampleConfirmationUpdate>(Assert.Single(clientProxy.Arguments));
        Assert.Equal("tx-001", payload.TransactionId);
        Assert.Equal(RusAuthConfirmationStatus.Success, payload.Status);
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

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }

    private sealed class FakeHubContext(RecordingClientProxy clientProxy) : IHubContext<ExampleConfirmationHub>
    {
        public IHubClients Clients { get; } = new FakeHubClients(clientProxy);
        public IGroupManager Groups { get; } = new FakeGroupManager();
    }

    private sealed class FakeHubClients(RecordingClientProxy clientProxy) : IHubClients
    {
        public IClientProxy All => clientProxy;
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => clientProxy;
        public IClientProxy Client(string connectionId) => clientProxy;
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => clientProxy;
        public IClientProxy Group(string groupName) => clientProxy;
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => clientProxy;
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => clientProxy;
        public IClientProxy User(string userId) => clientProxy;
        public IClientProxy Users(IReadOnlyList<string> userIds) => clientProxy;
    }

    private sealed class FakeGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RecordingClientProxy : IClientProxy
    {
        public string MethodName { get; private set; } = string.Empty;
        public object?[] Arguments { get; private set; } = [];

        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            MethodName = method;
            Arguments = args;
            return Task.CompletedTask;
        }
    }
}
