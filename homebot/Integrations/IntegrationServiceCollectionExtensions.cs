namespace HomeBot.Integrations;

public static class IntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddIntegration<TIntegration, TOptions>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TIntegration : class
        where TOptions : class, IIntegrationOptions
    {
        IConfigurationSection? configurationSection = null;

        try
        {
            configurationSection = configuration.GetRequiredSection(TOptions.Section);
        }
        catch (InvalidOperationException)
        {
            // todo: print error
        }

        if (configurationSection == null)
        {
            return services;
        }

        services.Configure<TOptions>(configurationSection);

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
        // register it as a metric provider
        if (typeof(IMetricProvider).IsAssignableFrom(typeof(TIntegration)))
        {
            services.AddSingleton(sp =>
                (IMetricProvider)sp.GetRequiredService<TIntegration>()
            );
        }


        return services;
    }
}