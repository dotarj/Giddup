// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.ApplicationCore.Application.PullRequests;

public interface IPullRequestService
{
    Task<IPullRequestError?> ProcessCommand(Guid pullRequestId, IPullRequestCommand command);
}
