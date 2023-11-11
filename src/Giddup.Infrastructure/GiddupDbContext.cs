// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using Giddup.Infrastructure.System.QueryModel;
using Giddup.Infrastructure.WorkItems.QueryModel;
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

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<WorkItem> WorkItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Event>(typeBuilder =>
        {
            _ = typeBuilder
                .HasIndex(@event => new { @event.AggregateId, @event.AggregateVersion })
                .IsUnique();

            _ = typeBuilder
                .Property(@event => @event.Offset)
                .UseIdentityAlwaysColumn();
        });

        _ = modelBuilder.Entity<OptionalReviewer>()
            .HasKey(optionalReviewer => new { optionalReviewer.UserId, optionalReviewer.PullRequestId });

        _ = modelBuilder.Entity<RequiredReviewer>()
            .HasKey(requiredReviewer => new { requiredReviewer.UserId, requiredReviewer.PullRequestId });

        _ = modelBuilder.Entity<User>()
            .HasData(
                new User { Id = Guid.Parse("6e76dc39-633a-422d-a922-626c3c220e6b"), FirstName = "Albert", LastName = "Einstein" },
                new User { Id = Guid.Parse("769f1cfe-eaab-4a4f-9776-755b89dfb973"), FirstName = "Isaac", LastName = "Newton" },
                new User { Id = Guid.Parse("e9faa5fd-2832-4d47-ac55-0655a20e274e"), FirstName = "Galileo", LastName = "Galilei" },
                new User { Id = Guid.Parse("8dd689c0-7c67-4936-8e89-c4e4896396bc"), FirstName = "Niels", LastName = "Bohr" });

        _ = modelBuilder.Entity<WorkItem>()
            .HasData(
                new WorkItem { Id = Guid.Parse("06256a69-6160-4f38-9bc0-4e255fae4087"), Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit." },
                new WorkItem { Id = Guid.Parse("ce037a68-9e4e-4a48-994b-f702dd63f102"), Title = "In congue erat lacus, vitae iaculis turpis accumsan vel." });
    }
}
