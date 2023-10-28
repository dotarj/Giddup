// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.ApplicationCore.PullRequests;

public interface IPullRequestStateProvider
{
    Task<(IPullRequestState State, ulong? Revision)> Provide(Guid pullRequestId);
}
