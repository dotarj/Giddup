// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestState
{
    public static IPullRequestState InitialState => new PullRequestInitialState();

    public static IPullRequestState ProcessEvent(IPullRequestState state, IPullRequestEvent @event)
    {
        if (state is PullRequestState createdState)
        {
            return @event switch
            {
                TargetBranchChangedEvent(var targetBranch) => createdState with { TargetBranch = targetBranch },
                TitleChangedEvent(var title) => createdState with { Title = title },
                DescriptionChangedEvent(var description) => createdState with { Description = description },

                RequiredReviewerAddedEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerAdded(userId, ReviewerType.Required) },
                OptionalReviewerAddedEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerAdded(userId, ReviewerType.Optional) },
                ReviewerMadeRequiredEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerTypeChanged(userId, ReviewerType.Required) },
                ReviewerMadeOptionalEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerTypeChanged(userId, ReviewerType.Optional) },
                ReviewerRemovedEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerRemoved(userId) },

                ApprovedEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Approved) },
                ApprovedWithSuggestionsEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.ApprovedWithSuggestions) },
                WaitingForAuthorEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.WaitingForAuthor) },
                RejectedEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Rejected) },
                FeedbackResetEvent(var userId) => createdState with { Reviewers = createdState.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.None) },

                WorkItemLinkedEvent(var workItemId) => createdState with { WorkItems = createdState.WorkItems.WithWorkItemLinked(workItemId) },
                WorkItemRemovedEvent(var workItemId) => createdState with { WorkItems = createdState.WorkItems.WithWorkItemRemoved(workItemId) },

                CompletedEvent => createdState with { Status = PullRequestStatus.Completed },
                AutoCompleteSetEvent => createdState with { AutoCompleteMode = AutoCompleteMode.Enabled },
                AutoCompleteCancelledEvent => createdState with { AutoCompleteMode = AutoCompleteMode.Disabled },
                AbandonedEvent => createdState with { Status = PullRequestStatus.Abandoned },
                ReactivatedEvent => createdState with { Status = PullRequestStatus.Active },

                _ => throw new InvalidOperationException($"State '{state.GetType().FullName}' and event '{@event.GetType().FullName}' not supported.")
            };
        }

        if (@event is not CreatedEvent createdEvent)
        {
            throw new InvalidOperationException($"State '{state.GetType().FullName}' and event '{@event.GetType().FullName}' not supported.");
        }

        return new PullRequestState(createdEvent.Owner, createdEvent.SourceBranch, createdEvent.TargetBranch, createdEvent.Title, string.Empty, CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode.Disabled, PullRequestStatus.Active, ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)>.Empty, ImmutableList<Guid>.Empty);
    }
}

public record PullRequestInitialState : IPullRequestState;

public record PullRequestState(Guid Owner, BranchName SourceBranch, BranchName TargetBranch, Title Title, string Description, CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode, AutoCompleteMode AutoCompleteMode, PullRequestStatus Status, ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> Reviewers, ImmutableList<Guid> WorkItems) : IPullRequestState;

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
