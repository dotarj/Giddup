// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel;
using Giddup.Infrastructure.PullRequests.QueryModel.Models;

namespace Giddup.Presentation.Api.Queries;

public class PullRequestQueries
{
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PullRequest> GetPullRequests([Service] IPullRequestQueryProcessor queryProcessor)
        => queryProcessor.Process();
}
