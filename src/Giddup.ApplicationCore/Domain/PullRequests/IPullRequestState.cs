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
            return new ExistingPullRequestState(createdEvent.OwnerId, createdEvent.SourceBranch, createdEvent.TargetBranch, createdEvent.Title, string.Empty, CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode.Disabled, PullRequestStatus.Active, ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)>.Empty, ImmutableList<Guid>.Empty);
        }

        if (state is not ExistingPullRequestState existingState)
        {
            throw new InvalidOperationException($"State '{state.GetType().FullName}' and event '{@event.GetType().FullName}' not supported.");
        }

        return @event switch
        {
            AbandonedEvent => existingState with { Status = PullRequestStatus.Abandoned },
            ApprovedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Approved) },
            ApprovedWithSuggestionsEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.ApprovedWithSuggestions) },
            AutoCompleteCancelledEvent => existingState with { AutoCompleteMode = AutoCompleteMode.Disabled },
            AutoCompleteSetEvent => existingState with { AutoCompleteMode = AutoCompleteMode.Enabled },
            CompletedEvent => existingState with { Status = PullRequestStatus.Completed },
            DescriptionChangedEvent(var description) => existingState with { Description = description },
            FeedbackResetEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.None) },
            OptionalReviewerAddedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerAdded(userId, ReviewerType.Optional) },
            ReactivatedEvent => existingState with { Status = PullRequestStatus.Active },
            RejectedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Rejected) },
            RequiredReviewerAddedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerAdded(userId, ReviewerType.Required) },
            ReviewerMadeOptionalEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerTypeChanged(userId, ReviewerType.Optional) },
            ReviewerMadeRequiredEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerTypeChanged(userId, ReviewerType.Required) },
            ReviewerRemovedEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerRemoved(userId) },
            TargetBranchChangedEvent(var targetBranch) => existingState with { TargetBranch = targetBranch },
            TitleChangedEvent(var title) => existingState with { Title = title },
            WaitingForAuthorEvent(var userId) => existingState with { Reviewers = existingState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.WaitingForAuthor) },
            WorkItemLinkedEvent(var workItemId) => existingState with { WorkItems = existingState.WorkItems.WithWorkItemLinked(workItemId) },
            WorkItemRemovedEvent(var workItemId) => existingState with { WorkItems = existingState.WorkItems.WithWorkItemRemoved(workItemId) },

            _ => throw new InvalidOperationException($"State '{state.GetType().FullName}' and event '{@event.GetType().FullName}' not supported.")
        };
    }
}

public record InitialPullRequestState : IPullRequestState;

public record ExistingPullRequestState(Guid OwnerId, BranchName SourceBranch, BranchName TargetBranch, Title Title, string Description, CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode, AutoCompleteMode AutoCompleteMode, PullRequestStatus Status, ImmutableList<(Guid ReviewerId, ReviewerType Type, ReviewerFeedback Feedback)> Reviewers, ImmutableList<Guid> WorkItems) : IPullRequestState;

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
