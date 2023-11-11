// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Giddup.ApplicationCore.Domain;
using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    private static IPullRequestState GetPullRequestState(Guid? CreatedById = null, BranchName? sourceBranch = null, BranchName? targetBranch = null, Title? title = null, string? description = null, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode = CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode autoCompleteMode = AutoCompleteMode.Disabled, PullRequestStatus status = PullRequestStatus.Active, ImmutableList<(Guid ReviewerId, ReviewerType Type, ReviewerFeedback Feedback)>? reviewers = null, ImmutableList<Guid>? workItems = null)
    {
        if (title is null)
        {
            _ = Title.TryCreate("title", out title!);
        }

        if (sourceBranch is null)
        {
            _ = BranchName.TryCreate("refs/heads/source", out sourceBranch!);
        }

        if (targetBranch is null)
        {
            _ = BranchName.TryCreate("refs/heads/target", out targetBranch!);
        }

        return new ExistingPullRequestState(CreatedById ?? Guid.NewGuid(), sourceBranch, targetBranch, title, description ?? "description", checkForLinkedWorkItemsMode, autoCompleteMode, status, reviewers ?? GetReviewers(), workItems ?? GetWorkItems());
    }

    private static ImmutableList<(Guid ReviewerId, ReviewerType Type, ReviewerFeedback Feedback)> GetReviewers(params (Guid ReviewerId, ReviewerType Type, ReviewerFeedback Feedback)[] reviewers)
        => reviewers
            .ToImmutableList();

    private static ImmutableList<Guid> GetWorkItems(params Guid[] workItems)
        => workItems
            .ToImmutableList();
}
