using Microsoft.Extensions.Options;

namespace HomeBot.Integrations;

public static class IntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddIntegration<TIntegration, TOptions>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TIntegration : class
        where TOptions : class, IIntegrationOptions
    {
        services.Configure<TOptions>(
            configuration.GetRequiredSection(TOptions.Section));

        var options = configuration
            .GetRequiredSection(TOptions.Section)
            .Get<TOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{TOptions.Section}' is missing.");

        if (!options.Enabled)
            return services;

        services.AddSingleton<TIntegration>();

        services.AddSingleton<IIntegration>(sp =>
            (IIntegration)sp.GetRequiredService<TIntegration>());

        return services;
    }
}