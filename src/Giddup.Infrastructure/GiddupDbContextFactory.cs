// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Giddup.Infrastructure;

public class GiddupDbContextFactory : IDesignTimeDbContextFactory<GiddupDbContext>
{
    public GiddupDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GiddupDbContext>();

        _ = optionsBuilder.UseNpgsql(string.Empty);

        return new GiddupDbContext(optionsBuilder.Options);
    }
}
