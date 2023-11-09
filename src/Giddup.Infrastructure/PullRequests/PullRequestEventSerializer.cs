// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using System.Text.Json;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.JsonConverters;

namespace Giddup.Infrastructure.PullRequests;

public static class PullRequestEventSerializer
{
    private static readonly Lazy<ImmutableList<Type>> PullRequestEventTypes = new(GetPullRequestEventTypes);
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    static PullRequestEventSerializer()
    {
        JsonSerializerOptions.Converters.Add(new BranchNameJsonConverter());
        JsonSerializerOptions.Converters.Add(new TitleJsonConverter());
    }

    public static IPullRequestEvent Deserialize(Event @event)
    {
        var eventType = GetPullRequestEventType(@event.Type);

        var deserializedEvent = (IPullRequestEvent?)JsonSerializer.Deserialize(@event.Data, eventType, JsonSerializerOptions);

        if (deserializedEvent == null)
        {
            throw new InvalidOperationException($"Empty event data found for event '{@event.Offset}'.");
        }

        return deserializedEvent;
    }

    public static string Serialize(IPullRequestEvent @event)
        => JsonSerializer.Serialize(@event, @event.GetType(), JsonSerializerOptions);

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
