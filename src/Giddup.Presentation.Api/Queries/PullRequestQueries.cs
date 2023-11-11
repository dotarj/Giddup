// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel;
using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using Giddup.Presentation.Api.Queries.FilterInputTypes;
using Giddup.Presentation.Api.Queries.SortInputTypes;

namespace Giddup.Presentation.Api.Queries;

public class PullRequestQueries
{
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering<PullRequestFilterInputType>]
    [UseSorting<PullRequestSortInputType>]
    public IQueryable<PullRequest> GetPullRequests([Service] IPullRequestQueryProcessor queryProcessor)
        => queryProcessor.Process();
}
