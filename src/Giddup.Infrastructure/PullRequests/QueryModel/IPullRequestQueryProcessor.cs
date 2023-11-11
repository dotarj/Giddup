// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;

namespace Giddup.Infrastructure.PullRequests.QueryModel;

public interface IPullRequestQueryProcessor
{
    IQueryable<PullRequest> Process();
}
