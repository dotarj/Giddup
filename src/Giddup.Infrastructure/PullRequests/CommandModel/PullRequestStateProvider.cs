// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.ApplicationCore.Domain.PullRequests;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Infrastructure.PullRequests.CommandModel;

public class PullRequestStateProvider : IPullRequestStateProvider
{
    private readonly GiddupDbContext _dbContext;

    public PullRequestStateProvider(GiddupDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<(IPullRequestState State, long? Version)> Provide(Guid pullRequestId)
    {
        var events = await _dbContext.Events
            .Where(@event => @event.AggregateId == pullRequestId)
            .Select(@event => new { Version = @event.AggregateVersion, Event = PullRequestEventSerializer.Deserialize(@event) })
            .ToListAsync();

        var state = events
            .Select(@event => @event.Event)
            .Aggregate(IPullRequestState.InitialState, IPullRequestState.ProcessEvent);

        var version = events
            .Select(@event => (long?)@event.Version)
            .LastOrDefault();

        return (state, version);
    }
}
