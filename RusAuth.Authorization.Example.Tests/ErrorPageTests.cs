namespace RusAuth.Authorization.Example.Tests;

using Bunit;
using Components.Pages;
using Contracts.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public sealed class ErrorPageTests : TestContext
{
    [Fact]
    public void Error_Page_Should_Render_Russian_Client_Facing_Guidance()
    {
        Services.AddSingleton<IOptions<RusAuthOptions>>(Options.Create(new RusAuthOptions
        {
            BaseUrl = "https://demo.rusauth.local/client-api/",
            Token = "company-token",
            TimeOut = 15
        }));

        var cut = RenderComponent<Error>();

        Assert.Contains("Не удалось выполнить запрос", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("RusAuth.BaseUrl", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("https://demo.rusauth.local/client-api/", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Example:CallbackBearerToken", cut.Markup, StringComparison.Ordinal);
    }
}