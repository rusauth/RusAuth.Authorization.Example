namespace RusAuth.Authorization.Example;

using Components;
using Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Configuration.AddJsonFile(
            $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
            optional: true,
            reloadOnChange: true);

        builder.Services.Configure<ForwardedHeadersOptions>(options => {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        var dataProtectionBuilder = builder.Services.AddDataProtection().SetApplicationName("RusAuth.Authorization.Example");

        var dataProtectionKeyRingPath = builder.Configuration["DataProtection:KeyRingPath"];

        if (!string.IsNullOrWhiteSpace(dataProtectionKeyRingPath))
        {
            Directory.CreateDirectory(dataProtectionKeyRingPath);
            dataProtectionBuilder.PersistKeysToFileSystem(new(dataProtectionKeyRingPath));
        }

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

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.MapStaticAssets();
        app.UseAntiforgery();

        app.MapHealthChecks("/health/live", new()
        {
            Predicate = static check => check.Tags.Contains("liveness")
        });

        app.MapHealthChecks("/health/ready", new()
        {
            Predicate = static check => check.Tags.Contains("readiness")
        });

        app.MapControllers();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.Run();
    }
}
