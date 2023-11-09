// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Presentation.Projections.AppStartup;

public static class EntityFrameworkCore
{
    public static IServiceCollection AddAppStartupEntityFrameworkCore(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDbContext<GiddupDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Postgres")));
}
