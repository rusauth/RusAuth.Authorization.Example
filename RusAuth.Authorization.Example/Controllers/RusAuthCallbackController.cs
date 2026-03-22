namespace RusAuth.Authorization.Example.Controllers;

using Contracts.Rest;
using Hubs;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;

[ApiController]
[Route("api/rusauth/callback")]
[ServiceFilter(typeof(CallbackBearerAuthorizationFilter))]
public sealed class RusAuthCallbackController(
    ExampleConfirmationStore store,
    IHubContext<ExampleConfirmationHub> hubContext,
    ILogger<RusAuthCallbackController> logger) : ControllerBase
{
    [HttpPost("confirmation")]
    public async Task<IActionResult> Confirmation([FromBody] RusAuthConfirmationWebHookRequest request)
    {
        var flow = store.MarkCallbackReceived(request);
        if (flow is null)
        {
            logger.LogWarning("Callback received for unknown transaction {TransactionId}", request.TransactionId);
            return NotFound();
        }

        logger.LogInformation("Received RusAuth callback for transaction {TransactionId}", request.TransactionId);
        await hubContext.Clients.Group(ExampleConfirmationHub.UpdatesGroup)
                        .SendAsync(ExampleConfirmationHub.ConfirmationUpdatedMethod,
                                   new ExampleConfirmationUpdate
                                   {
                                       TransactionId = flow.TransactionId,
                                       Status = flow.Status,
                                       CallbackReceivedOn = flow.CallbackReceivedOn,
                                       CompletedOn = flow.CompletedOn
                                   });

        return Ok();
    }
}