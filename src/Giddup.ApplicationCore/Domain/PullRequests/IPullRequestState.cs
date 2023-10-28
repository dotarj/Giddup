// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestState
{
    public static IPullRequestState InitialState => new InitialPullRequestState();

    public static IPullRequestState ProcessEvent(IPullRequestState state, IPullRequestEvent @event)
    {
        if (@event is CreatedEvent createdEvent)
        {
            return new ExistingPullRequestState(createdEvent.Owner, createdEvent.SourceBranch, createdEvent.TargetBranch, createdEvent.Title, string.Empty, CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode.Disabled, PullRequestStatus.Active, ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)>.Empty, ImmutableList<Guid>.Empty);
        }

        if (state is not ExistingPullRequestState existingState)
        {
            throw new InvalidOperationException();
        }

        return @event switch
        {
            TargetBranchChangedEvent(var targetBranch) => existingState with { TargetBranch = targetBranch },
            TitleChangedEvent(var title) => existingState with { Title = title },
            DescriptionChangedEvent(var description) => existingState with { Description = description },

            RequiredReviewerAddedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerAdded(userId, ReviewerType.Required) },
            OptionalReviewerAddedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerAdded(userId, ReviewerType.Optional) },
            ReviewerMadeRequiredEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerTypeChanged(userId, ReviewerType.Required) },
            ReviewerMadeOptionalEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerTypeChanged(userId, ReviewerType.Optional) },
            ReviewerRemovedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerRemoved(userId) },

            ApprovedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Approved) },
            ApprovedWithSuggestionsEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.ApprovedWithSuggestions) },
            WaitingForAuthorEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.WaitingForAuthor) },
            RejectedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Rejected) },
            FeedbackResetEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.None) },

            WorkItemLinkedEvent(var workItemId) => existingState with { WorkItems = existingState.WorkItems.WithWorkItemLinked(workItemId) },
            WorkItemRemovedEvent(var workItemId) => existingState with { WorkItems = existingState.WorkItems.WithWorkItemRemoved(workItemId) },

            CompletedEvent => existingState with { Status = PullRequestStatus.Completed },
            AutoCompleteSetEvent => existingState with { AutoCompleteMode = AutoCompleteMode.Enabled },
            AutoCompleteCancelledEvent => existingState with { AutoCompleteMode = AutoCompleteMode.Disabled },
            AbandonedEvent => existingState with { Status = PullRequestStatus.Abandoned },
            ReactivatedEvent => existingState with { Status = PullRequestStatus.Active },

            _ => throw new InvalidOperationException($"State '{state.GetType().FullName}' and event '{@event.GetType().FullName}' not supported.")
        };
    }
}

public record InitialPullRequestState : IPullRequestState;

public record ExistingPullRequestState(Guid Owner, BranchName SourceBranch, BranchName TargetBranch, Title Title, string Description, CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode, AutoCompleteMode AutoCompleteMode, PullRequestStatus Status, ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> Reviewers, ImmutableList<Guid> WorkItems) : IPullRequestState;

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
