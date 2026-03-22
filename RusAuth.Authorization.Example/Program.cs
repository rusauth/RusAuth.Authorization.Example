namespace RusAuth.Authorization.Example;

using Components;
using Hubs;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        builder.Services.AddControllers();
        builder.Services.AddHealthChecks()
               .AddCheck("self", static () => HealthCheckResult.Healthy(), ["liveness", "readiness"]);
        builder.Services.AddRusAuthExample(builder.Configuration);

        var app = builder.Build();

        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.MapStaticAssets();
        app.UseAntiforgery();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = static check => check.Tags.Contains("liveness")
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = static check => check.Tags.Contains("readiness")
        });
        app.MapControllers();
        app.MapHub<ExampleConfirmationHub>("/hubs/example-confirmations");
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.Run();
    }
}
