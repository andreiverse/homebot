using System.Reflection;

namespace HomeBot.Integrations;

public static class IntegrationServiceCollectionExtensions
{
    
    private static void RegisterImplementations(
        IServiceCollection services,
        Assembly assembly,
        Type openGenericInterface)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;

                if (iface.GetGenericTypeDefinition() != openGenericInterface)
                    continue;

                services.AddSingleton(type);
                services.AddSingleton(iface, sp => sp.GetRequiredService(type));
            }
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


        RegisterImplementations(
            services,
            typeof(TIntegration).Assembly,
            typeof(IIntegrationWidget<>));



        return services;
    }
}