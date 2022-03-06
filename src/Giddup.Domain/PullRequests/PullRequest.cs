// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Domain.PullRequests;

public static class PullRequest
{
    public static PullRequestState InitialState => new PullRequestInitialState();

    public static DeciderResult<IPullRequestEvent, IPullRequestError> Decide(PullRequestState state, IPullRequestCommand command)
    {
        if (state.TryPickT0(out _, out var createdState))
        {
            if (command is not CreateCommand(var sourceBranch, var targetBranch, var title))
            {
                return new NotCreatedError();
            }

            return new CreatedEvent(sourceBranch, targetBranch, title);
        }

        return command switch
        {
            CreateCommand => new AlreadyCreatedError(),

            ChangeTitleCommand(var title) => ChangeTitle(title, createdState),
            ChangeDescriptionCommand(var description) => ChangeDescription(description, createdState),

            AddRequiredReviewerCommand(var userId) => AddRequiredReviewer(userId, createdState),
            AddOptionalReviewerCommand(var userId) => AddOptionalReviewer(userId, createdState),
            MakeReviewerRequiredCommand(var userId) => MakeReviewerRequired(userId, createdState),
            MakeReviewerOptionalCommand(var userId) => MakeReviewerOptional(userId, createdState),
            RemoveReviewerCommand(var userId) => RemoveReviewer(userId, createdState),

            ApproveCommand(var userId) => Approve(userId, createdState),
            ApproveWithSuggestionsCommand(var userId) => ApproveWithSuggestions(userId, createdState),
            WaitForAuthorCommand(var userId) => WaitForAuthor(userId, createdState),
            RejectCommand(var userId) => Reject(userId, createdState),
            ResetFeedbackCommand(var userId) => ResetFeedback(userId, createdState),

            LinkWorkItemCommand(var workItemId) => LinkWorkItem(workItemId, createdState),
            RemoveWorkItemCommand(var workItemId) => RemoveWorkItem(workItemId, createdState),

            CompleteCommand => Complete(createdState),
            SetAutoCompleteCommand => SetAutoComplete(createdState),
            CancelAutoCompleteCommand => CancelAutoComplete(createdState),
            AbandonCommand => Abandon(createdState),
            ReactivateCommand => Reactivate(createdState),

            _ => throw new InvalidOperationException($"Command '{command.GetType().FullName}' not supported.")
        };
    }

    public static PullRequestState Evolve(PullRequestState state, IPullRequestEvent @event)
    {
        if (state.TryPickT0(out _, out var createdState))
        {
            if (@event is not CreatedEvent(var sourceBranch, var targetBranch, var title))
            {
                throw new InvalidOperationException($"State '{state.GetType().FullName}' and event '{@event.GetType().FullName}' not supported.");
            }

            return new PullRequestCreatedState(sourceBranch, targetBranch, title, string.Empty, CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode.Disabled, PullRequestStatus.Active, new List<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)>().AsReadOnly(), new List<Guid>().AsReadOnly());
        }

        return @event switch
        {
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

    private static DeciderResult<IPullRequestEvent, IPullRequestError> ChangeTitle(Title title, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (title == state.Title)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new TitleChangedEvent(title);
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> ChangeDescription(string description, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (description == state.Description)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new DescriptionChangedEvent(description);
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> AddRequiredReviewer(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.UserId == userId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new RequiredReviewerAddedEvent(userId);
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> AddOptionalReviewer(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.UserId == userId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new OptionalReviewerAddedEvent(userId);
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> MakeReviewerRequired(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var reviewer = state.Reviewers.FirstOrDefault(reviewer => reviewer.UserId == userId);

        if (reviewer == default)
        {
            return new ReviewerNotFoundError(userId);
        }

        if (reviewer.Type == ReviewerType.Required)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new ReviewerMadeRequiredEvent(userId);
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> MakeReviewerOptional(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var reviewer = state.Reviewers.FirstOrDefault(reviewer => reviewer.UserId == userId);

        if (reviewer == default)
        {
            return new ReviewerNotFoundError(userId);
        }

        if (reviewer.Type == ReviewerType.Optional)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new ReviewerMadeOptionalEvent(userId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerTypeChanged(reviewer.UserId, ReviewerType.Optional) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> RemoveReviewer(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.All(reviewer => reviewer.UserId != userId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new ReviewerRemovedEvent(userId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerRemoved(userId) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> Approve(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();
        var reviewers = state.Reviewers;

        if (reviewers.All(reviewer => reviewer.UserId != userId))
        {
            events.Add(new OptionalReviewerAddedEvent(userId));

            reviewers = reviewers.WithReviewerAdded(userId, ReviewerType.Optional);
        }
        else
        {
            reviewers = reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.Approved);
        }

        events.Add(new ApprovedEvent(userId));

        if (ShouldAutoComplete(state with { Reviewers = reviewers }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> ApproveWithSuggestions(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();
        var reviewers = state.Reviewers;

        if (reviewers.All(reviewer => reviewer.UserId != userId))
        {
            events.Add(new OptionalReviewerAddedEvent(userId));

            reviewers = reviewers.WithReviewerAdded(userId, ReviewerType.Optional);
        }
        else
        {
            reviewers = reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.ApprovedWithSuggestions);
        }

        events.Add(new ApprovedWithSuggestionsEvent(userId));

        if (ShouldAutoComplete(state with { Reviewers = reviewers }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> WaitForAuthor(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();

        if (state.Reviewers.All(reviewer => reviewer.UserId != userId))
        {
            events.Add(new OptionalReviewerAddedEvent(userId));
        }

        events.Add(new WaitingForAuthorEvent(userId));

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> Reject(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();

        if (state.Reviewers.All(reviewer => reviewer.UserId != userId))
        {
            events.Add(new OptionalReviewerAddedEvent(userId));
        }

        events.Add(new RejectedEvent(userId));

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> ResetFeedback(Guid userId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.All(reviewer => reviewer.UserId != userId))
        {
            return new ReviewerNotFoundError(userId);
        }

        var events = new List<IPullRequestEvent> { new FeedbackResetEvent(userId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerFeedbackChanged(userId, ReviewerFeedback.None) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> LinkWorkItem(Guid workItemId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.WorkItems.Any(workItem => workItem == workItemId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new WorkItemLinkedEvent(workItemId) };

        if (ShouldAutoComplete(state with { WorkItems = state.WorkItems.WithWorkItemLinked(workItemId) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> RemoveWorkItem(Guid workItemId, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.WorkItems.All(workItem => workItem != workItemId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new WorkItemRemovedEvent(workItemId);
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> Complete(PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.Feedback is ReviewerFeedback.WaitingForAuthor or ReviewerFeedback.Rejected))
        {
            return new FeedbackContainsWaitForAuthorOrRejectError();
        }

        var requiredReviewers = state.Reviewers.Where(reviewer => reviewer.Type == ReviewerType.Required);

        if (!requiredReviewers.All(reviewer => reviewer.Feedback is ReviewerFeedback.Approved or ReviewerFeedback.ApprovedWithSuggestions))
        {
            return new NotAllRequiredReviewersApprovedError();
        }

        if (state.CheckForLinkedWorkItemsMode == CheckForLinkedWorkItemsMode.Enabled && state.WorkItems.Count == 0)
        {
            return new NoWorkItemLinkedError();
        }

        return new CompletedEvent();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> SetAutoComplete(PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.AutoCompleteMode == AutoCompleteMode.Enabled)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new AutoCompleteSetEvent() };

        if (ShouldAutoComplete(state with { AutoCompleteMode = AutoCompleteMode.Enabled }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> CancelAutoComplete(PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.AutoCompleteMode == AutoCompleteMode.Disabled)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new AutoCompleteCancelledEvent();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> Abandon(PullRequestCreatedState state)
    {
        if (state.Status == PullRequestStatus.Abandoned)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        return new AbandonedEvent();
    }

    private static DeciderResult<IPullRequestEvent, IPullRequestError> Reactivate(PullRequestCreatedState state)
    {
        if (state.Status == PullRequestStatus.Active)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (state.Status != PullRequestStatus.Abandoned)
        {
            return new NotAbandonedError();
        }

        return new ReactivatedEvent();
    }

    private static bool ShouldAutoComplete(PullRequestCreatedState state)
    {
        if (state.AutoCompleteMode == AutoCompleteMode.Disabled)
        {
            return false;
        }

        if (state.Reviewers.Any(reviewer => reviewer.Feedback is ReviewerFeedback.WaitingForAuthor or ReviewerFeedback.Rejected))
        {
            return false;
        }

        var requiredReviewers = state.Reviewers.Where(reviewer => reviewer.Type == ReviewerType.Required);

        if (!requiredReviewers.All(reviewer => reviewer.Feedback is ReviewerFeedback.Approved or ReviewerFeedback.ApprovedWithSuggestions))
        {
            return false;
        }

        if (state.CheckForLinkedWorkItemsMode == CheckForLinkedWorkItemsMode.Enabled && state.WorkItems.Count == 0)
        {
            return false;
        }

        return true;
    }

    private static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerAdded(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerType type)
        => reviewers
            .Append((userId, type, ReviewerFeedback.None))
            .ToList()
            .AsReadOnly();

    private static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerFeedbackChanged(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerFeedback feedback)
        => reviewers
            .Select(reviewer => reviewer.UserId == userId ? new(reviewer.UserId, reviewer.Type, feedback) : reviewer)
            .ToList()
            .AsReadOnly();

    private static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerRemoved(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId)
        => reviewers
            .Where(reviewer => reviewer.UserId != userId)
            .ToList()
            .AsReadOnly();

    private static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerTypeChanged(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerType type)
        => reviewers
            .Select(reviewer => reviewer.UserId.Equals(userId) ? new(reviewer.UserId, type, reviewer.Feedback) : reviewer)
            .ToList()
            .AsReadOnly();

    private static IReadOnlyCollection<Guid> WithWorkItemLinked(this IReadOnlyCollection<Guid> workItems, Guid workItemId)
        => workItems
            .Append(workItemId)
            .ToList()
            .AsReadOnly();

    private static IReadOnlyCollection<Guid> WithWorkItemRemoved(this IReadOnlyCollection<Guid> workItems, Guid workItemId)
        => workItems
            .Where(existingWorkItemId => existingWorkItemId != workItemId)
            .ToList()
            .AsReadOnly();
}
