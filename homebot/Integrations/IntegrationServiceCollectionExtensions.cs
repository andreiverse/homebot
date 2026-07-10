using Microsoft.Extensions.Options;

namespace HomeBot.Integrations;

public static class IntegrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers an integration's HTTP client as a typed client via <c>IHttpClientFactory</c>,
    /// configured from the integration's options, instead of each integration constructing its
    /// own <see cref="HttpClient"/>.
    /// </summary>
    public static IServiceCollection AddIntegrationHttpClient<THttpClient, TOptions>(
        this IServiceCollection services,
        Action<HttpClient, TOptions> configure)
        where THttpClient : class
        where TOptions : class, IIntegrationOptions
    {
        services.AddHttpClient<THttpClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<TOptions>>().CurrentValue;
            configure(http, options);
        });

        return services;
    }

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
        // register it as a metric provider
        if (typeof(IMetricProvider).IsAssignableFrom(typeof(TIntegration))) {
            services.AddSingleton(sp =>
                (IMetricProvider)sp.GetRequiredService<TIntegration>()
            );
        }

        return services;
    }
}