// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Presentation.Api.Queries;

public class PullRequestQueries
{
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PullRequest> GetPullRequests()
    {
        throw new NotImplementedException();
    }

    public record PullRequest(Guid Id);
}
