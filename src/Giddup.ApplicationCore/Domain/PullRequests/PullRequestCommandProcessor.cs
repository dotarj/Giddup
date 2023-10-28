// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public static class PullRequestCommandProcessor
{
    public static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Process(IPullRequestState state, IPullRequestCommand command)
    {
        if (state is PullRequestCreatedState createdState)
        {
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

        if (command is not CreateCommand createCommand)
        {
            return new NotCreatedError();
        }

        return new CreatedEvent(createCommand.Owner, createCommand.SourceBranch, createCommand.TargetBranch, createCommand.Title);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeTitle(Title title, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeDescription(string description, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> AddRequiredReviewer(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> AddOptionalReviewer(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerRequired(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerOptional(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveReviewer(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Approve(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ApproveWithSuggestions(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> WaitForAuthor(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Reject(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ResetFeedback(Guid userId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> LinkWorkItem(Guid workItemId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveWorkItem(Guid workItemId, PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Complete(PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> SetAutoComplete(PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> CancelAutoComplete(PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Abandon(PullRequestCreatedState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Reactivate(PullRequestCreatedState state)
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
}
