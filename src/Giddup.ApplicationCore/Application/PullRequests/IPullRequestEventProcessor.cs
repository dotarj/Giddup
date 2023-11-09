// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.ApplicationCore.Application.PullRequests;

public interface IPullRequestEventProcessor
{
    Task<bool> Process(Guid pullRequestId, long? expectedVersion, ImmutableList<IPullRequestEvent> events);
}
