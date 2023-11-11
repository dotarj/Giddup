// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Giddup.Presentation.Api.AppStartup;

public static class AspNetCore
{
    public static IServiceCollection AddAppStartupAspNetCore(this IServiceCollection services)
    {
        _ = services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        return services;
    }

    public static WebApplication UseAppStartupAspNetCore(this WebApplication app)
    {
        _ = app.MapControllers();

        return app;
    }
}
