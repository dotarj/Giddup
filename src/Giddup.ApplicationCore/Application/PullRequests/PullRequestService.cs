// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.ApplicationCore.Application.PullRequests;

public class PullRequestService : IPullRequestService
{
    private readonly IPullRequestStateProvider _stateProvider;
    private readonly IPullRequestEventProcessor _eventProcessor;

    public PullRequestService(IPullRequestStateProvider stateProvider, IPullRequestEventProcessor eventProcessor)
    {
        _stateProvider = stateProvider;
        _eventProcessor = eventProcessor;
    }

    public async Task<IPullRequestError?> ProcessCommand(Guid pullRequestId, IPullRequestCommand command)
    {
        var (state, version) = await _stateProvider.Provide(pullRequestId);

        var result = await PullRequestCommandProcessor.Process(state, command);

        if (!result.TryGetEvents(out var events, out var error))
        {
            return error;
        }

        if (!await _eventProcessor.Process(pullRequestId, version, events))
        {
            return new OptimisticConcurrencyCheckError();
        }

        return null;
    }
}
