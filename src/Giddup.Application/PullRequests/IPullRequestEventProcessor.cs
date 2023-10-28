// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;

namespace Giddup.Application.PullRequests;

public interface IPullRequestEventProcessor
{
    Task Process(Guid pullRequestId, ulong? revision, IReadOnlyCollection<IPullRequestEvent> events);
}
