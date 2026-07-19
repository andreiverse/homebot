using System.Reflection;

namespace HomeBot.Integrations;

public static class IntegrationServiceCollectionExtensions
{
    /// <summary>
    /// Scans <paramref name="assembly"/> for all concrete <see cref="IWidget"/> implementations
    /// and registers each as transient under both its concrete type and <see cref="IWidget"/>.
    /// Transient lifetime is required because widgets have mutable properties that are set
    /// per-request before calling <see cref="IWidget.RenderAsync"/>.
    /// </summary>
    private static void RegisterWidgets(IServiceCollection services, Assembly assembly, Type integrationType)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(IWidget).IsAssignableFrom(type))
                continue;

            if (type.Namespace == null || integrationType.Namespace == null || !type.Namespace.StartsWith(integrationType.Namespace))
                continue;

            // Register the concrete type so modules can inject e.g. JellyfinServerInfoWidget.
            services.AddTransient(type);

            // Also register as IWidget so consumers can resolve IEnumerable<IWidget>.
            services.AddTransient(typeof(IWidget), sp => sp.GetRequiredService(type));
        }
    }

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
            return services;

        services.Configure<TOptions>(configurationSection);

        var options = configuration
            .GetRequiredSection(TOptions.Section)
            .Get<TOptions>();

        if (options == null || !options.Enabled)
            return services;

        // Register the integration as its concrete type.
        services.AddSingleton<TIntegration>();

        // Register as IIntegration for generic consumers.
        services.AddSingleton(sp => (IIntegration)sp.GetRequiredService<TIntegration>());

        // Register as IMetricProvider if applicable.
        if (typeof(IMetricProvider).IsAssignableFrom(typeof(TIntegration)))
        {
            services.AddSingleton(sp =>
                (IMetricProvider)sp.GetRequiredService<TIntegration>());
        }

        RegisterWidgets(services, typeof(TIntegration).Assembly, typeof(TIntegration));

        return services;
    }
}