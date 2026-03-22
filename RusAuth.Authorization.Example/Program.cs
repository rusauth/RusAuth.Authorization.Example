namespace RusAuth.Authorization.Example;

using Components;
using Hubs;
using Infrastructure;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        builder.Services.AddControllers();
        builder.Services.AddRusAuthExample(builder.Configuration);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.MapStaticAssets();
        app.UseAntiforgery();
        app.MapControllers();
        app.MapHub<ExampleConfirmationHub>("/hubs/example-confirmations");
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.Run();
    }
}