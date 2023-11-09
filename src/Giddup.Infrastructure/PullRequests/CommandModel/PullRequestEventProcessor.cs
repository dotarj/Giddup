// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.ApplicationCore.Domain.PullRequests;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Infrastructure.PullRequests.CommandModel;

public class PullRequestEventProcessor : IPullRequestEventProcessor
{
    private readonly GiddupDbContext _dbContext;

    public PullRequestEventProcessor(GiddupDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<bool> Process(Guid pullRequestId, long? expectedVersion, ImmutableList<IPullRequestEvent> events)
    {
        var currentVersion = await GetCurrentVersion(pullRequestId);

        if (currentVersion != expectedVersion)
        {
            return false;
        }

        var version = currentVersion ?? 0;

        _dbContext.Events
            .AddRange(events
                .Select(@event => new Event
                {
                    AggregateId = pullRequestId,
                    AggregateVersion = ++version,
                    Type = @event.GetType().Name,
                    Data = PullRequestEventSerializer.Serialize(@event)
                }));

        _ = await _dbContext.SaveChangesAsync();

        return true;
    }

    private Task<long?> GetCurrentVersion(Guid pullRequestId)
        => _dbContext.Events
            .Where(@event => @event.AggregateId == pullRequestId)
            .Select(@event => @event.AggregateVersion)
            .MaxAsync(aggregateVersion => (long?)aggregateVersion);
}
