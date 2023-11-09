// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.Infrastructure.PullRequests.QueryModel.Models;

public class PullRequest
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string SourceBranch { get; set; } = string.Empty;

    public string TargetBranch { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PullRequestStatus Status { get; set; }

    public AutoCompleteMode AutoCompleteMode { get; set; }

    public CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode { get; set; }

    public List<OptionalReviewer> OptionalReviewers { get; set; } = new();

    public List<RequiredReviewer> RequiredReviewers { get; set; } = new();
}
