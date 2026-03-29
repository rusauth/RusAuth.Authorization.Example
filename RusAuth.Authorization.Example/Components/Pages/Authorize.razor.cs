namespace RusAuth.Authorization.Example.Components.Pages;

using System.ComponentModel.DataAnnotations;
using Contracts;
using Infrastructure;
using Microsoft.AspNetCore.Components;
using Services;

public partial class Authorize : IDisposable
{
    [Inject]
    private ExampleAuthFacade Facade { get; set; } = default!;

    [Inject]
    private ExampleAuthSession Session { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IExampleConfirmationNotifier ConfirmationNotifier { get; set; } = default!;

    [Inject]
    private ILogger<Authorize> Logger { get; set; } = default!;

    protected AuthorizeFormModel Model { get; } = new();
    protected bool IsBusy { get; private set; }
    protected string? LocalError { get; private set; }
    protected ExampleConfirmationFlow? CurrentFlow => Facade.GetCurrentFlow();

    public void Dispose() => ConfirmationNotifier.ConfirmationUpdated -= OnConfirmationUpdatedAsync;

    protected override async Task OnInitializedAsync()
    {
        ConfirmationNotifier.ConfirmationUpdated += OnConfirmationUpdatedAsync;
        Logger.LogInformation("Authorize page initialized. BaseUri={BaseUri}", NavigationManager.BaseUri);
        await Task.CompletedTask;
    }

    protected async Task SubmitAsync() =>
        await ExecuteAsync(async () => {
            Logger.LogInformation("Submitting confirmation request. PhoneNumber={PhoneNumber} ExpirationMinute={ExpirationMinute}",
                                  Model.PhoneNumber.ToDisplayString(),
                                  Model.ExpirationMinute);
            await Facade.StartConfirmationAsync(BuildPhoneNumber(),
                                                GetCallbackUrl(),
                                                Model.ExpirationMinute);
        });

    protected async Task CheckStatusAsync() =>
        await ExecuteAsync(async () => {
            Logger.LogInformation("Submitting manual confirmation status check. PhoneNumber={PhoneNumber}",
                                  Model.PhoneNumber.ToDisplayString());
            await Facade.CheckCurrentConfirmationAsync(BuildPhoneNumber());
        });

    protected Task RefreshAsync()
    {
        Session.ClearMessages();
        LocalError = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected string GetStatusCssClass(CallConfirmationStatus status) =>
        status switch
        {
            CallConfirmationStatus.Success => "status-pill status-pill-success",
            CallConfirmationStatus.Failed  => "status-pill status-pill-failed",
            CallConfirmationStatus.Expired => "status-pill status-pill-expired",
            _                              => "status-pill status-pill-pending"
        };

    protected string GetStatusText(CallConfirmationStatus status) =>
        status.ToDisplayString();

    protected static string FormatDate(DateTime? value) =>
        value?.ToLocalTime().ToString("u") ?? "ещё нет";

    protected string GetCallbackUrl() => Facade.BuildCallbackUrl(new(NavigationManager.BaseUri));

    private string BuildPhoneNumber() => Model.PhoneNumber;

    private async Task OnConfirmationUpdatedAsync(ExampleConfirmationUpdate update)
    {
        var currentTransactionId = CurrentFlow?.TransactionId ?? Session.CurrentTransactionId;
        if (!string.Equals(update.TransactionId, currentTransactionId, StringComparison.Ordinal))
            return;

        Logger.LogInformation("Received matching confirmation update. TransactionId={TransactionId} Status={Status}",
                              update.TransactionId,
                              update.Status);
        Session.SetInfo("Вебхук получен. Статус подтверждения обновлён автоматически.");
        LocalError = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        LocalError = null;
        IsBusy = true;
        Session.ClearMessages();

        try
        {
            await action();
            Logger.LogInformation("Authorize page action completed successfully. CurrentTransactionId={TransactionId}",
                                  CurrentFlow?.TransactionId ?? Session.CurrentTransactionId);
        }
        catch (Exception ex)
        {
            Session.SetError(ex.Message);
            LocalError = ex.Message;
            Logger.LogError(ex, "Authorize page action failed.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected sealed class AuthorizeFormModel
    {
        [Required]
        [RegularExpression(@"^\d{5,15}$", ErrorMessage = "Введите только цифры без пробелов и разделителей.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Range(1, 60)]
        public int ExpirationMinute { get; set; } = 10;
    }
}