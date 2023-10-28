// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.Infrastructure.PullRequests;

public class PullRequestEventProcessor : IPullRequestEventProcessor
{
    private readonly IEventStream _eventStream;

    public PullRequestEventProcessor(IEventStream eventStream)
        => _eventStream = eventStream;

    public Task Process(Guid pullRequestId, ulong? revision, ImmutableList<IPullRequestEvent> events)
    {
        var streamName = $"pull-request-{pullRequestId}";

        return _eventStream.AppendToStream(streamName, revision, events);
    }
}
