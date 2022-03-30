// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using Giddup.Domain.PullRequests;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    private static IPullRequestState GetPullRequestState(Guid? owner = null, BranchName? sourceBranch = null, BranchName? targetBranch = null, Title? title = null, string? description = null, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode = CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode autoCompleteMode = AutoCompleteMode.Disabled, PullRequestStatus status = PullRequestStatus.Active, IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)>? reviewers = null, IReadOnlyCollection<Guid>? workItems = null)
    {
        if (title is null)
        {
            _ = Title.TryCreate("title", out title!, out _);
        }

        if (sourceBranch is null)
        {
            _ = BranchName.TryCreate("refs/heads/source", out sourceBranch!, out _);
        }

        if (targetBranch is null)
        {
            _ = BranchName.TryCreate("refs/heads/target", out targetBranch!, out _);
        }

        return new PullRequestCreatedState(owner ?? Guid.NewGuid(), sourceBranch, targetBranch, title, description ?? "description", checkForLinkedWorkItemsMode, autoCompleteMode, status, reviewers ?? GetReviewers(), workItems ?? GetWorkItems());
    }

    private static ReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> GetReviewers(params (Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)[] reviewers)
        => reviewers
            .ToList()
            .AsReadOnly();

    private static ReadOnlyCollection<Guid> GetWorkItems(params Guid[] workItems)
        => workItems
            .ToList()
            .AsReadOnly();
}
