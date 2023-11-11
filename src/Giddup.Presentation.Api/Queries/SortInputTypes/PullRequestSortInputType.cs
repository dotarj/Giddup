// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using HotChocolate.Data.Sorting;

namespace Giddup.Presentation.Api.Queries.SortInputTypes;

public class PullRequestSortInputType : SortInputType<PullRequest>
{
    protected override void Configure(ISortInputTypeDescriptor<PullRequest> descriptor)
    {
        descriptor.Field(sample => sample.Title);
    }
}
