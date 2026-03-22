namespace RusAuth.Authorization.Example.Components.Pages;

using Contracts.Rest;
using Infrastructure;
using Microsoft.AspNetCore.Components;
using Services;

public partial class Account : IDisposable
{
    [Inject]
    private ExampleAuthFacade Facade { get; set; } = default!;

    [Inject]
    private ExampleAuthSession Session { get; set; } = default!;

    [Inject]
    private IExampleConfirmationSignalRClient ConfirmationSignalRClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected IReadOnlyCollection<ExampleConfirmationFlow> Flows => Facade.GetTrackedFlows();
    protected string? CurrentTransactionId => Session.CurrentTransactionId;
    protected string CallbackBearerTokenPreview => Facade.GetCallbackBearerTokenPreview();

    public void Dispose() => ConfirmationSignalRClient.ConfirmationUpdated -= OnConfirmationUpdatedAsync;

    protected override async Task OnInitializedAsync()
    {
        ConfirmationSignalRClient.ConfirmationUpdated += OnConfirmationUpdatedAsync;
        await ConfirmationSignalRClient.EnsureConnectedAsync(new(NavigationManager.BaseUri));
    }

    protected Task RefreshAsync()
    {
        Session.ClearMessages();
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected string GetStatusCssClass(RusAuthConfirmationStatus? status) =>
        status switch
        {
            RusAuthConfirmationStatus.Success => "status-pill status-pill-success",
            RusAuthConfirmationStatus.Failed  => "status-pill status-pill-failed",
            RusAuthConfirmationStatus.Expired => "status-pill status-pill-expired",
            _                                 => "status-pill status-pill-pending"
        };

    protected string GetStatusText(RusAuthConfirmationStatus? status) =>
        status?.ToDisplayString() ?? "ожидается подтверждение";

    protected static string FormatDate(DateTime? value) =>
        value?.ToLocalTime().ToString("u") ?? "ещё нет";

    private async Task OnConfirmationUpdatedAsync(ExampleConfirmationUpdate update)
    {
        Session.SetInfo($"Получён callback по транзакции {update.TransactionId}. Журнал обновлён.");
        await InvokeAsync(StateHasChanged);
    }
}