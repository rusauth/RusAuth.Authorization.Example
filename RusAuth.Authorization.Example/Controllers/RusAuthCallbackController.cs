namespace RusAuth.Authorization.Example.Controllers;

using Contracts.Rest;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Services;

[ApiController]
[Route("api/rusauth/callback")]
[ServiceFilter(typeof(CallbackBearerAuthorizationFilter))]
public sealed class RusAuthCallbackController(
    ExampleConfirmationStore store,
    IExampleConfirmationNotifier notifier,
    ILogger<RusAuthCallbackController> logger) : ControllerBase
{
    [HttpPost("confirmation")]
    public async Task<IActionResult> Confirmation([FromBody] RusAuthConfirmationWebHookRequest request)
    {
        logger.LogInformation("RusAuth callback received. TransactionId={TransactionId} RemoteIp={RemoteIp}",
                              request.TransactionId,
                              HttpContext.Connection.RemoteIpAddress?.ToString());
        var flow = store.MarkCallbackReceived(request);
        if (flow is null)
        {
            logger.LogWarning("Callback received for unknown transaction {TransactionId}", request.TransactionId);
            return NotFound();
        }

        logger.LogInformation("Received RusAuth callback for transaction {TransactionId}. Publishing update to interactive pages.",
                              request.TransactionId);
        await notifier.PublishAsync(new()
        {
            TransactionId = flow.TransactionId,
            Status = flow.Status,
            CallbackReceivedOn = flow.CallbackReceivedOn,
            CompletedOn = flow.CompletedOn
        });

        return Ok();
    }
}