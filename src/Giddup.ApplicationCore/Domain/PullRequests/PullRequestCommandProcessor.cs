// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public static class PullRequestCommandProcessor
{
    public static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> Process(IPullRequestState state, IPullRequestCommand command)
    {
        if (command is CreateCommand createCommand)
        {
            if (state is not InitialPullRequestState)
            {
                return new AlreadyExistsError();
            }

            return await Create(createCommand);
        }

        if (state is not ExistingPullRequestState existingState)
        {
            return new NotFoundError();
        }

        return command switch
        {
            ChangeTargetBranchCommand changeTargetBranchCommand => await ChangeTargetBranch(changeTargetBranchCommand, existingState),
            ChangeTitleCommand changeTitleCommand => ChangeTitle(changeTitleCommand, existingState),
            ChangeDescriptionCommand changeDescriptionCommand => ChangeDescription(changeDescriptionCommand, existingState),

            AddRequiredReviewerCommand addRequiredReviewerCommand => await AddRequiredReviewer(addRequiredReviewerCommand, existingState),
            AddOptionalReviewerCommand addOptionalReviewerCommand => await AddOptionalReviewer(addOptionalReviewerCommand, existingState),
            MakeReviewerRequiredCommand makeReviewerRequiredCommand => MakeReviewerRequired(makeReviewerRequiredCommand, existingState),
            MakeReviewerOptionalCommand makeReviewerOptionalCommand => MakeReviewerOptional(makeReviewerOptionalCommand, existingState),
            RemoveReviewerCommand removeReviewerCommand => RemoveReviewer(removeReviewerCommand, existingState),

            ApproveCommand approveCommand => Approve(approveCommand, existingState),
            ApproveWithSuggestionsCommand approveWithSuggestionsCommand => ApproveWithSuggestions(approveWithSuggestionsCommand, existingState),
            WaitForAuthorCommand waitForAuthorCommand => WaitForAuthor(waitForAuthorCommand, existingState),
            RejectCommand rejectCommand => Reject(rejectCommand, existingState),
            ResetFeedbackCommand resetFeedbackCommand => ResetFeedback(resetFeedbackCommand, existingState),

            LinkWorkItemCommand linkWorkItemCommand => LinkWorkItem(linkWorkItemCommand, existingState),
            RemoveWorkItemCommand removeWorkItemCommand => RemoveWorkItem(removeWorkItemCommand, existingState),

            CompleteCommand => Complete(existingState),
            SetAutoCompleteCommand => SetAutoComplete(existingState),
            CancelAutoCompleteCommand => CancelAutoComplete(existingState),
            AbandonCommand => Abandon(existingState),
            ReactivateCommand => Reactivate(existingState),

            _ => throw new InvalidOperationException($"Command '{command.GetType().FullName}' not supported.")
        };
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

        if (command.TargetBranch == command.SourceBranch)
        {
            return new TargetBranchEqualsSourceBranchError();
        }

        return new CreatedEvent(command.Owner, command.SourceBranch, command.TargetBranch, command.Title);
    }

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> ChangeTargetBranch(ChangeTargetBranchCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (command.TargetBranch == state.TargetBranch)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (command.TargetBranch == state.SourceBranch)
        {
            return new TargetBranchEqualsSourceBranchError();
        }

        if (!await command.IsExistingBranch(command.TargetBranch))
        {
            return new InvalidTargetBranchError();
        }

        return new TargetBranchChangedEvent(command.TargetBranch);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeTitle(ChangeTitleCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeDescription(ChangeDescriptionCommand command, ExistingPullRequestState state)
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

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> AddRequiredReviewer(AddRequiredReviewerCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.UserId == command.UserId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (!await command.IsValidReviewer(command.UserId))
        {
            return new InvalidReviewerError();
        }

        return new RequiredReviewerAddedEvent(command.UserId);
    }

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> AddOptionalReviewer(AddOptionalReviewerCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.UserId == command.UserId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (!await command.IsValidReviewer(command.UserId))
        {
            return new InvalidReviewerError();
        }

        return new OptionalReviewerAddedEvent(command.UserId);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerRequired(MakeReviewerRequiredCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerOptional(MakeReviewerOptionalCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveReviewer(RemoveReviewerCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Approve(ApproveCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ApproveWithSuggestions(ApproveWithSuggestionsCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> WaitForAuthor(WaitForAuthorCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Reject(RejectCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ResetFeedback(ResetFeedbackCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> LinkWorkItem(LinkWorkItemCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveWorkItem(RemoveWorkItemCommand command, ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Complete(ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> SetAutoComplete(ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> CancelAutoComplete(ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Abandon(ExistingPullRequestState state)
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Reactivate(ExistingPullRequestState state)
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

    private static bool ShouldAutoComplete(ExistingPullRequestState state)
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
