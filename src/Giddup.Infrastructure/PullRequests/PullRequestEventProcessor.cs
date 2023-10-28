// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.ApplicationCore.PullRequests;

namespace Giddup.Infrastructure.PullRequests;

public class PullRequestEventProcessor : IPullRequestEventProcessor
{
    private readonly IEventStream _eventStream;

    public PullRequestEventProcessor(IEventStream eventStream)
        => _eventStream = eventStream;

    public Task Process(Guid pullRequestId, ulong? revision, IReadOnlyCollection<IPullRequestEvent> events)
    {
        var streamName = $"pull-request-{pullRequestId}";

        return _eventStream.AppendToStream(streamName, revision, events);
    }
}
