// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;

namespace Giddup.Application.PullRequests;

public interface IPullRequestStateProvider
{
    Task<(IPullRequestState State, ulong? Revision)> Provide(Guid pullRequestId);
}
