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
            .Get<TOptions>(); 

        if (options == null || !options.Enabled)
            return services;

        // register specific Type
        services.AddSingleton<TIntegration>();
        // register Interface
        services.AddSingleton(sp =>
            (IIntegration)sp.GetRequiredService<TIntegration>());

        return services;
    }
}