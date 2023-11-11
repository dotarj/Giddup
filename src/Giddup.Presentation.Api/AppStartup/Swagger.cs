// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Microsoft.OpenApi.Models;

namespace Giddup.Presentation.Api.AppStartup;

public static class Swagger
{
    public static IServiceCollection AddAppStartupSwagger(this IServiceCollection services)
        => services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();

                options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Giddup API" });
            });

    public static WebApplication UseAppStartupSwagger(this WebApplication app)
    {
        _ = app
            .UseSwagger()
            .UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Giddup API v1"));

        return app;
    }
}
