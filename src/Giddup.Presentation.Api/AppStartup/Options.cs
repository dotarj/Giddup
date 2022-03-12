// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure;

namespace Giddup.Presentation.Api.AppStartup;

public static class Options
{
    public static IServiceCollection AddAppStartupOptions(this IServiceCollection services, ConfigurationManager configuration)
    {
        _ = services
            .AddOptions<EventStoreClientOptions>()
            .Bind(configuration.GetSection(nameof(EventStoreClientOptions)))
            .ValidateDataAnnotations();

        return services;
    }
}
