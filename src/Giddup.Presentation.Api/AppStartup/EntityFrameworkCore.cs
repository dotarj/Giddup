// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Presentation.Api.AppStartup;

public static class EntityFrameworkCore
{
    public static IServiceCollection AddAppStartupEntityFrameworkCore(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDbContext<GiddupDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Postgres")));

    public static WebApplication UseAppStartupEntityFrameworkCore(this WebApplication app, bool isDevelopment)
    {
        if (isDevelopment)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<GiddupDbContext>();

            dbContext.Database.Migrate();
        }

        return app;
    }
}
