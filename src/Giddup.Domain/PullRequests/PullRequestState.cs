// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using OneOf;

namespace Giddup.Domain.PullRequests;

[GenerateOneOf]
public partial class PullRequestState : OneOfBase<PullRequestInitialState, PullRequestCreatedState>
{
}

public record PullRequestInitialState;

public record PullRequestCreatedState(Guid Owner, BranchName SourceBranch, BranchName TargetBranch, Title Title, string Description, CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode, AutoCompleteMode AutoCompleteMode, PullRequestStatus Status, IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> Reviewers, IReadOnlyCollection<Guid> WorkItems);

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
