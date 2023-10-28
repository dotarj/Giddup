// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;

namespace Giddup.Infrastructure;

public interface IEventStream
{
    Task AppendToStream<TEvent>(string name, ulong? expectedRevision, ImmutableList<TEvent> events)
        where TEvent : notnull;

    Task<(ulong? Revision, ImmutableList<TEvent> Events)> ReadStream<TEvent>(string streamName)
        where TEvent : notnull;
}
