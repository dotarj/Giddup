// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public static class PullRequestCommandProcessor
{
    public static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> Process(IPullRequestState state, IPullRequestCommand command)
    {
        if (state is PullRequestCreatedState createdState)
        {
            return command switch
            {
                CreateCommand => new AlreadyCreatedError(),

                ChangeTargetBranchCommand changeTargetBranchCommand => await ChangeTargetBranch(changeTargetBranchCommand, createdState),
                ChangeTitleCommand changeTitleCommand => ChangeTitle(changeTitleCommand, createdState),
                ChangeDescriptionCommand changeDescriptionCommand => ChangeDescription(changeDescriptionCommand, createdState),

                AddRequiredReviewerCommand addRequiredReviewerCommand => AddRequiredReviewer(addRequiredReviewerCommand, createdState),
                AddOptionalReviewerCommand addOptionalReviewerCommand => AddOptionalReviewer(addOptionalReviewerCommand, createdState),
                MakeReviewerRequiredCommand makeReviewerRequiredCommand => MakeReviewerRequired(makeReviewerRequiredCommand, createdState),
                MakeReviewerOptionalCommand makeReviewerOptionalCommand => MakeReviewerOptional(makeReviewerOptionalCommand, createdState),
                RemoveReviewerCommand removeReviewerCommand => RemoveReviewer(removeReviewerCommand, createdState),

                ApproveCommand approveCommand => Approve(approveCommand, createdState),
                ApproveWithSuggestionsCommand approveWithSuggestionsCommand => ApproveWithSuggestions(approveWithSuggestionsCommand, createdState),
                WaitForAuthorCommand waitForAuthorCommand => WaitForAuthor(waitForAuthorCommand, createdState),
                RejectCommand rejectCommand => Reject(rejectCommand, createdState),
                ResetFeedbackCommand resetFeedbackCommand => ResetFeedback(resetFeedbackCommand, createdState),

                LinkWorkItemCommand linkWorkItemCommand => LinkWorkItem(linkWorkItemCommand, createdState),
                RemoveWorkItemCommand removeWorkItemCommand => RemoveWorkItem(removeWorkItemCommand, createdState),

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

        return await Create(createCommand);
    }

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> Create(CreateCommand command)
    {
        if (!await command.IsExistingBranch(command.SourceBranch))
        {
            return new InvalidSourceBranchError();
        }

        if (!await command.IsExistingBranch(command.TargetBranch))
        {
            return new InvalidTargetBranchError();
        }

        return new CreatedEvent(command.Owner, command.SourceBranch, command.TargetBranch, command.Title);
    }

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> ChangeTargetBranch(ChangeTargetBranchCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (command.TargetBranch == state.TargetBranch)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (!await command.IsExistingBranch(command.TargetBranch))
        {
            return new InvalidTargetBranchError();
        }

        return new TargetBranchChangedEvent(command.TargetBranch);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeTitle(ChangeTitleCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (command.Title == state.Title)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new TitleChangedEvent(command.Title);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeDescription(ChangeDescriptionCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (command.Description == state.Description)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new DescriptionChangedEvent(command.Description);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> AddRequiredReviewer(AddRequiredReviewerCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.UserId == command.UserId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new RequiredReviewerAddedEvent(command.UserId);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> AddOptionalReviewer(AddOptionalReviewerCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.UserId == command.UserId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new OptionalReviewerAddedEvent(command.UserId);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerRequired(MakeReviewerRequiredCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var reviewer = state.Reviewers.FirstOrDefault(reviewer => reviewer.UserId == command.UserId);

        if (reviewer == default)
        {
            return new ReviewerNotFoundError(command.UserId);
        }

        if (reviewer.Type == ReviewerType.Required)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new ReviewerMadeRequiredEvent(command.UserId);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerOptional(MakeReviewerOptionalCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var reviewer = state.Reviewers.FirstOrDefault(reviewer => reviewer.UserId == command.UserId);

        if (reviewer == default)
        {
            return new ReviewerNotFoundError(command.UserId);
        }

        if (reviewer.Type == ReviewerType.Optional)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new ReviewerMadeOptionalEvent(command.UserId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerTypeChanged(reviewer.UserId, ReviewerType.Optional) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveReviewer(RemoveReviewerCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.All(reviewer => reviewer.UserId != command.UserId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new ReviewerRemovedEvent(command.UserId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerRemoved(command.UserId) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Approve(ApproveCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();
        var reviewers = state.Reviewers;

        if (reviewers.All(reviewer => reviewer.UserId != command.UserId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.UserId));

            reviewers = reviewers.WithReviewerAdded(command.UserId, ReviewerType.Optional);
        }
        else
        {
            reviewers = reviewers.WithReviewerFeedbackChanged(command.UserId, ReviewerFeedback.Approved);
        }

        events.Add(new ApprovedEvent(command.UserId));

        if (ShouldAutoComplete(state with { Reviewers = reviewers }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ApproveWithSuggestions(ApproveWithSuggestionsCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();
        var reviewers = state.Reviewers;

        if (reviewers.All(reviewer => reviewer.UserId != command.UserId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.UserId));

            reviewers = reviewers.WithReviewerAdded(command.UserId, ReviewerType.Optional);
        }
        else
        {
            reviewers = reviewers.WithReviewerFeedbackChanged(command.UserId, ReviewerFeedback.ApprovedWithSuggestions);
        }

        events.Add(new ApprovedWithSuggestionsEvent(command.UserId));

        if (ShouldAutoComplete(state with { Reviewers = reviewers }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> WaitForAuthor(WaitForAuthorCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();

        if (state.Reviewers.All(reviewer => reviewer.UserId != command.UserId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.UserId));
        }

        events.Add(new WaitingForAuthorEvent(command.UserId));

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Reject(RejectCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();

        if (state.Reviewers.All(reviewer => reviewer.UserId != command.UserId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.UserId));
        }

        events.Add(new RejectedEvent(command.UserId));

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ResetFeedback(ResetFeedbackCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.All(reviewer => reviewer.UserId != command.UserId))
        {
            return new ReviewerNotFoundError(command.UserId);
        }

        var events = new List<IPullRequestEvent> { new FeedbackResetEvent(command.UserId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerFeedbackChanged(command.UserId, ReviewerFeedback.None) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> LinkWorkItem(LinkWorkItemCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.WorkItems.Any(workItem => workItem == command.WorkItemId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new WorkItemLinkedEvent(command.WorkItemId) };

        if (ShouldAutoComplete(state with { WorkItems = state.WorkItems.WithWorkItemLinked(command.WorkItemId) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveWorkItem(RemoveWorkItemCommand command, PullRequestCreatedState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.WorkItems.All(workItem => workItem != command.WorkItemId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new WorkItemRemovedEvent(command.WorkItemId);
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
