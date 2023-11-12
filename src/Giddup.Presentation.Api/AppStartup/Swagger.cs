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

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid JWT",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

    public static WebApplication UseAppStartupSwagger(this WebApplication app)
    {
        _ = app
            .UseSwagger()
            .UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Giddup API v1"));

        return app;
    }
}
