// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using Giddup.Domain.PullRequests;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    private static PullRequestState GetPullRequestState(BranchName? sourceBranch = null, BranchName? targetBranch = null, Title? title = null, string? description = null, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode = CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode autoCompleteMode = AutoCompleteMode.Disabled, PullRequestStatus status = PullRequestStatus.Active, IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)>? reviewers = null, IReadOnlyCollection<Guid>? workItems = null)
        => new PullRequestCreatedState(sourceBranch ?? BranchName.Create("refs/heads/source").AsT1, targetBranch ?? BranchName.Create("refs/heads/target").AsT1, title ?? Title.Create("title").AsT1, description ?? "description", checkForLinkedWorkItemsMode, autoCompleteMode, status, reviewers ?? GetReviewers(), workItems ?? GetWorkItems());

    private static ReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> GetReviewers(params (Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)[] reviewers)
        => reviewers
            .ToList()
            .AsReadOnly();

    private static ReadOnlyCollection<Guid> GetWorkItems(params Guid[] workItems)
        => workItems
            .ToList()
            .AsReadOnly();
}
