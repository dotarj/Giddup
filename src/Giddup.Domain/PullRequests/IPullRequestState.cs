// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Domain.PullRequests;

public interface IPullRequestState
{
}

public record PullRequestInitialState : IPullRequestState;

public record PullRequestCreatedState(Guid Owner, BranchName SourceBranch, BranchName TargetBranch, Title Title, string Description, CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode, AutoCompleteMode AutoCompleteMode, PullRequestStatus Status, IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> Reviewers, IReadOnlyCollection<Guid> WorkItems) : IPullRequestState;

public enum CheckForLinkedWorkItemsMode
{
    Disabled,
    Enabled
}

public enum AutoCompleteMode
{
    Disabled,
    Enabled
}

public enum PullRequestStatus
{
    Active,
    Completed,
    Abandoned
}

public enum ReviewerType
{
    Optional,
    Required
}

public enum ReviewerFeedback
{
    None,
    Approved,
    ApprovedWithSuggestions,
    WaitingForAuthor,
    Rejected
}
