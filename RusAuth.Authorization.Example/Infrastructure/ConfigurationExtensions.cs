namespace RusAuth.Authorization.Example.Infrastructure;

using Contracts.Rest;
using Extensions;
using Services;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddRusAuthExample(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RusAuthOptions>()
                .Bind(configuration.GetRequiredSection("RusAuth"))
                .ValidateDataAnnotations()
                .Validate(static options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                          "RusAuth:BaseUrl must be an absolute URI.")
                .ValidateOnStart();

        services.AddOptions<ExampleCallbackOptions>()
                .Bind(configuration.GetRequiredSection("Example"))
                .Validate(static options => !string.IsNullOrWhiteSpace(options.CallbackBearerToken),
                          "Example:CallbackBearerToken is required.")
                .ValidateOnStart();

        services.AddRusAuthConfirmationClient(configuration.GetRequiredConfiguration<RusAuthOptions>("RusAuth"));
        services.AddScoped<CallbackBearerAuthorizationFilter>();
        services.AddSingleton<ExampleConfirmationStore>();
        services.AddSingleton<IExampleConfirmationNotifier, ExampleConfirmationNotifier>();
        services.AddScoped<ExampleAuthSession>();
        services.AddScoped<ExampleAuthFacade>();

        return services;
    }

    public static T GetRequiredConfiguration<T>(this IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        var value = configuration.GetRequiredSection(sectionName).Get<T>();
        return value ?? throw new InvalidOperationException($"Configuration section '{sectionName}' is missing or invalid.");
    }
}
