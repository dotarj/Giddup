// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.ApplicationCore.Application.PullRequests;

public interface IPullRequestStateProvider
{
    Task<(IPullRequestState State, long? Version)> Provide(Guid pullRequestId);
}
