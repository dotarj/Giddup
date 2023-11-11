// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using HotChocolate.Data.Filters;

namespace Giddup.Presentation.Api.Queries.FilterInputTypes;

public class PullRequestFilterInputType : FilterInputType<PullRequest>
{
    protected override void Configure(IFilterInputTypeDescriptor<PullRequest> descriptor)
    {
        descriptor.Field(pullRequest => pullRequest.Id);
        descriptor.Field(pullRequest => pullRequest.Owner);
        descriptor.Field(pullRequest => pullRequest.Status);
        descriptor.Field(pullRequest => pullRequest.TargetBranch);
    }
}
