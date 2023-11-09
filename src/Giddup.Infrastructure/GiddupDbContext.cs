// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Infrastructure;

public class GiddupDbContext : DbContext
{
    public GiddupDbContext(DbContextOptions<GiddupDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; } = null!;

    public DbSet<EventProjectionOffset> EventProjectionOffsets { get; set; } = null!;

    public DbSet<PullRequest> PullRequests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Event>()
            .HasIndex(@event => new { @event.AggregateId, @event.AggregateVersion })
            .IsUnique();

        _ = modelBuilder.Entity<Event>()
            .Property(@event => @event.Offset)
            .UseIdentityAlwaysColumn();
    }
}
