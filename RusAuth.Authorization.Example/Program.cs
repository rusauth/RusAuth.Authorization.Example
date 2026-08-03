namespace RusAuth.Authorization.Example;

using Components;
using Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Security.Cryptography.X509Certificates;

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
        var dataProtectionCertificatePath = builder.Configuration["DataProtection:CertificatePath"];
        var dataProtectionCertificateKeyPath = builder.Configuration["DataProtection:CertificateKeyPath"];

        if (!string.IsNullOrWhiteSpace(dataProtectionKeyRingPath))
        {
            Directory.CreateDirectory(dataProtectionKeyRingPath);
            dataProtectionBuilder.PersistKeysToFileSystem(new(dataProtectionKeyRingPath));
        }

        if (string.IsNullOrWhiteSpace(dataProtectionCertificatePath) !=
            string.IsNullOrWhiteSpace(dataProtectionCertificateKeyPath))
        {
            throw new InvalidOperationException(
                "DataProtection certificate and private-key paths must be configured together.");
        }

        if (!string.IsNullOrWhiteSpace(dataProtectionCertificatePath))
        {
            var dataProtectionCertificate = X509Certificate2.CreateFromPemFile(
                dataProtectionCertificatePath,
                dataProtectionCertificateKeyPath);
            dataProtectionBuilder.ProtectKeysWithCertificate(dataProtectionCertificate);
        }

        if (builder.Environment.IsProduction() &&
            (string.IsNullOrWhiteSpace(dataProtectionKeyRingPath) ||
             string.IsNullOrWhiteSpace(dataProtectionCertificatePath)))
        {
            throw new InvalidOperationException(
                "Production requires a persistent Data Protection key ring encrypted with a dedicated certificate.");
        }

        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        builder.Services.AddControllers();
        builder.Services.AddHealthChecks()
               .AddCheck("self", static () => HealthCheckResult.Healthy(), ["liveness", "readiness"]);

        builder.Services.AddRusAuthExample(builder.Configuration);

        var app = builder.Build();

        app.UseForwardedHeaders();

        var healthManagementPort = builder.Configuration.GetValue<int>("HealthChecks:ManagementPort");

        if (healthManagementPort > 0)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/health") &&
                    context.Connection.LocalPort != healthManagementPort)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await next(context);
            });
        }

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
