// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using System.Text.Json;
using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.JsonConverters;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Infrastructure.PullRequests;

public class PullRequestStateProvider : IPullRequestStateProvider
{
    private static readonly Lazy<ImmutableList<Type>> PullRequestEventTypes = new(GetPullRequestEventTypes);

    private readonly JsonSerializerOptions _jsonSerializerOptions = new();

    private readonly GiddupDbContext _dbContext;

    public PullRequestStateProvider(GiddupDbContext dbContext)
    {
        _jsonSerializerOptions.Converters.Add(new BranchNameJsonConverter());
        _jsonSerializerOptions.Converters.Add(new TitleJsonConverter());

        _dbContext = dbContext;
    }

    public async Task<(IPullRequestState State, long? Version)> Provide(Guid pullRequestId)
    {
        var events = await _dbContext.Events
            .Where(@event => @event.AggregateId == pullRequestId)
            .Select(@event => new { Version = @event.AggregateVersion, Event = DeserializeEvent(@event, _jsonSerializerOptions) })
            .ToListAsync();

        var state = events
            .Select(@event => @event.Event)
            .Aggregate(IPullRequestState.InitialState, IPullRequestState.ProcessEvent);

        var version = events
            .Select(@event => (long?)@event.Version)
            .LastOrDefault();

        return (state, version);
    }

    private static IPullRequestEvent DeserializeEvent(Event @event, JsonSerializerOptions jsonSerializerOptions)
    {
        var eventType = GetPullRequestEventType(@event.Type);

        var deserializedEvent = (IPullRequestEvent?)JsonSerializer.Deserialize(@event.Data, eventType, jsonSerializerOptions);

        if (deserializedEvent == null)
        {
            throw new InvalidOperationException($"Empty event data found for event '{@event.Id}'.");
        }

        return deserializedEvent;
    }

    private static Type GetPullRequestEventType(string name)
    {
        var type = PullRequestEventTypes.Value
            .FirstOrDefault(type => type.Name == name);

        if (type == null)
        {
            throw new InvalidOperationException($"Event '{name}' not supported.");
        }

        return type;
    }

    private static ImmutableList<Type> GetPullRequestEventTypes()
        => typeof(IPullRequestEvent).Assembly
            .GetTypes()
            .Where(type => typeof(IPullRequestEvent).IsAssignableFrom(type))
            .ToImmutableList();
}
