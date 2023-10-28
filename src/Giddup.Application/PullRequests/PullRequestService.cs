// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;

namespace Giddup.Application.PullRequests;

public class PullRequestService : IPullRequestService
{
    private readonly IEventStream _eventStream;

    public PullRequestService(IEventStream eventStream) => _eventStream = eventStream;

    public async Task<IPullRequestError?> Execute(Guid pullRequestId, IPullRequestCommand command)
    {
        var streamName = $"pull-request-{pullRequestId}";
        var (streamRevision, streamEvents) = await _eventStream.ReadStream<IPullRequestEvent>(streamName);

        var state = streamEvents.Aggregate(IPullRequestState.InitialState, IPullRequestState.ProcessEvent);

        var result = PullRequestCommandProcessor.Process(state, command);

        if (!result.TryGetEvents(out var events, out var error))
        {
            return error;
        }

        await _eventStream.AppendToStream(streamName, streamRevision, events);

        return null;
    }
}
