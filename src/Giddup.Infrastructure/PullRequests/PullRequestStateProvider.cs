// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.ApplicationCore.PullRequests;

namespace Giddup.Infrastructure.PullRequests;

public class PullRequestStateProvider : IPullRequestStateProvider
{
    private readonly IEventStream _eventStream;

    public PullRequestStateProvider(IEventStream eventStream)
        => _eventStream = eventStream;

    public async Task<(IPullRequestState State, ulong? Revision)> Provide(Guid pullRequestId)
    {
        var streamName = $"pull-request-{pullRequestId}";
        var (streamRevision, streamEvents) = await _eventStream.ReadStream<IPullRequestEvent>(streamName);

        var state = streamEvents.Aggregate(IPullRequestState.InitialState, IPullRequestState.ProcessEvent);

        return (state, streamRevision);
    }
}
