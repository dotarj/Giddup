// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Text.Json;
using EventStore.Client;
using Giddup.Application;
using Giddup.Infrastructure.JsonConverters;
using Microsoft.Extensions.Options;

namespace Giddup.Infrastructure;

public class EventStream : IEventStream
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new();

    private readonly EventStoreClient _client;

    public EventStream(IOptions<EventStoreClientOptions> options)
    {
        _jsonSerializerOptions.Converters.Add(new BranchNameJsonConverter());
        _jsonSerializerOptions.Converters.Add(new TitleJsonConverter());

        _client = new EventStoreClient(EventStoreClientSettings.Create(options.Value.ConnectionString));
    }

    public async Task AppendToStream<TEvent>(string name, ulong? expectedRevision, IReadOnlyCollection<TEvent> events)
        where TEvent : notnull
    {
        var eventData = events.Select(@event => new EventData(Uuid.NewUuid(), EventMapping.Value.GetEventName(@event.GetType()), JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _jsonSerializerOptions)));

        _ = expectedRevision.HasValue
            ? await _client.AppendToStreamAsync(name, expectedRevision.Value, eventData)
            : await _client.AppendToStreamAsync(name, StreamState.NoStream, eventData);
    }

    public async Task<(ulong? Revision, IReadOnlyCollection<TEvent> Events)> ReadStream<TEvent>(string name)
        where TEvent : notnull
    {
        var result = _client.ReadStreamAsync(Direction.Forwards, name, StreamPosition.Start);

        if (await result.ReadState == ReadState.StreamNotFound)
        {
            return (null, new List<TEvent>().AsReadOnly());
        }

        var events = await result
            .Select(resolvedEvent =>
            {
                var eventNumber = resolvedEvent.Event.EventNumber.ToUInt64();
                var @event = (TEvent)JsonSerializer.Deserialize(resolvedEvent.Event.Data.Span, EventMapping.Value.GetEventType(resolvedEvent.Event.EventType), _jsonSerializerOptions)!;

                return (EventNumber: eventNumber, Event: @event);
            })
            .ToListAsync();

        return (events.Last().EventNumber, events.Select(@event => @event.Event).ToList().AsReadOnly());
    }
}
