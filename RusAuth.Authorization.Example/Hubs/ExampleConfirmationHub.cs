namespace RusAuth.Authorization.Example.Hubs;

using Microsoft.AspNetCore.SignalR;

public sealed class ExampleConfirmationHub : Hub
{
    public const string UpdatesGroup = "example-confirmation-updates";
    public const string ConfirmationUpdatedMethod = "ConfirmationUpdated";

    public Task SubscribeToUpdatesAsync() =>
        Groups.AddToGroupAsync(Context.ConnectionId, UpdatesGroup);

    public Task UnsubscribeFromUpdatesAsync() =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, UpdatesGroup);
}