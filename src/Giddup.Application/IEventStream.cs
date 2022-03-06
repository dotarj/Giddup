// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Application;

public interface IEventStream
{
    Task AppendToStream<TEvent>(string name, ulong? expectedRevision, IReadOnlyCollection<TEvent> events)
        where TEvent : notnull;

    Task<(ulong? Revision, IReadOnlyCollection<TEvent> Events)> ReadStream<TEvent>(string streamName)
        where TEvent : notnull;
}
