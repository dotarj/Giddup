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
            AbandonCommand => Abandon(existingState),
            AddOptionalReviewerCommand addOptionalReviewerCommand => await AddOptionalReviewer(addOptionalReviewerCommand, existingState),
            AddRequiredReviewerCommand addRequiredReviewerCommand => await AddRequiredReviewer(addRequiredReviewerCommand, existingState),
            ApproveCommand approveCommand => Approve(approveCommand, existingState),
            ApproveWithSuggestionsCommand approveWithSuggestionsCommand => ApproveWithSuggestions(approveWithSuggestionsCommand, existingState),
            CancelAutoCompleteCommand => CancelAutoComplete(existingState),
            ChangeDescriptionCommand changeDescriptionCommand => ChangeDescription(changeDescriptionCommand, existingState),
            ChangeTargetBranchCommand changeTargetBranchCommand => await ChangeTargetBranch(changeTargetBranchCommand, existingState),
            ChangeTitleCommand changeTitleCommand => ChangeTitle(changeTitleCommand, existingState),
            CompleteCommand => Complete(existingState),
            LinkWorkItemCommand linkWorkItemCommand => LinkWorkItem(linkWorkItemCommand, existingState),
            MakeReviewerOptionalCommand makeReviewerOptionalCommand => MakeReviewerOptional(makeReviewerOptionalCommand, existingState),
            MakeReviewerRequiredCommand makeReviewerRequiredCommand => MakeReviewerRequired(makeReviewerRequiredCommand, existingState),
            ReactivateCommand => Reactivate(existingState),
            RejectCommand rejectCommand => Reject(rejectCommand, existingState),
            RemoveReviewerCommand removeReviewerCommand => RemoveReviewer(removeReviewerCommand, existingState),
            RemoveWorkItemCommand removeWorkItemCommand => RemoveWorkItem(removeWorkItemCommand, existingState),
            ResetFeedbackCommand resetFeedbackCommand => ResetFeedback(resetFeedbackCommand, existingState),
            SetAutoCompleteCommand => SetAutoComplete(existingState),
            WaitForAuthorCommand waitForAuthorCommand => WaitForAuthor(waitForAuthorCommand, existingState),

            _ => throw new InvalidOperationException($"Command '{command.GetType().FullName}' not supported.")
        };
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

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> AddOptionalReviewer(AddOptionalReviewerCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.ReviewerId == command.ReviewerId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (!await command.IsValidReviewer(command.ReviewerId))
        {
            return new InvalidReviewerError();
        }

        return new OptionalReviewerAddedEvent(command.ReviewerId);
    }

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> AddRequiredReviewer(AddRequiredReviewerCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.Any(reviewer => reviewer.ReviewerId == command.ReviewerId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (!await command.IsValidReviewer(command.ReviewerId))
        {
            return new InvalidReviewerError();
        }

        return new RequiredReviewerAddedEvent(command.ReviewerId);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Approve(ApproveCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();
        var reviewers = state.Reviewers;

        if (reviewers.All(reviewer => reviewer.ReviewerId != command.ReviewerId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.ReviewerId));

            reviewers = reviewers.WithReviewerAdded(command.ReviewerId, ReviewerType.Optional);
        }
        else
        {
            reviewers = reviewers.WithReviewerFeedbackChanged(command.ReviewerId, ReviewerFeedback.Approved);
        }

        events.Add(new ApprovedEvent(command.ReviewerId));

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

        if (reviewers.All(reviewer => reviewer.ReviewerId != command.ReviewerId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.ReviewerId));

            reviewers = reviewers.WithReviewerAdded(command.ReviewerId, ReviewerType.Optional);
        }
        else
        {
            reviewers = reviewers.WithReviewerFeedbackChanged(command.ReviewerId, ReviewerFeedback.ApprovedWithSuggestions);
        }

        events.Add(new ApprovedWithSuggestionsEvent(command.ReviewerId));

        if (ShouldAutoComplete(state with { Reviewers = reviewers }))
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

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> ChangeTargetBranch(ChangeTargetBranchCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (!BranchName.TryCreate(command.TargetBranch, out var targetBranch))
        {
            return new InvalidBranchNameError();
        }

        if (targetBranch == state.TargetBranch)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        if (targetBranch == state.SourceBranch)
        {
            return new TargetBranchEqualsSourceBranchError();
        }

        if (!await command.IsExistingBranch(targetBranch))
        {
            return new InvalidTargetBranchError();
        }

        return new TargetBranchChangedEvent(targetBranch);
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ChangeTitle(ChangeTitleCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (!Title.TryCreate(command.Title, out var title))
        {
            return new InvalidTitleError();
        }

        if (title == state.Title)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new TitleChangedEvent(title);
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

    private static async Task<CommandProcessorResult<IPullRequestEvent, IPullRequestError>> Create(CreateCommand command)
    {
        if (!BranchName.TryCreate(command.SourceBranch, out var sourceBranch))
        {
            return new InvalidBranchNameError();
        }

        if (!await command.IsExistingBranch(sourceBranch))
        {
            return new InvalidSourceBranchError();
        }

        if (!BranchName.TryCreate(command.TargetBranch, out var targetBranch))
        {
            return new InvalidBranchNameError();
        }

        if (!await command.IsExistingBranch(targetBranch))
        {
            return new InvalidTargetBranchError();
        }

        if (targetBranch == sourceBranch)
        {
            return new TargetBranchEqualsSourceBranchError();
        }

        if (!Title.TryCreate(command.Title, out var title))
        {
            return new InvalidTitleError();
        }

        return new CreatedEvent(command.CreatedById, sourceBranch, targetBranch, title);
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerOptional(MakeReviewerOptionalCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var reviewer = state.Reviewers.FirstOrDefault(reviewer => reviewer.ReviewerId == command.ReviewerId);

        if (reviewer == default)
        {
            return new ReviewerNotFoundError(command.ReviewerId);
        }

        if (reviewer.Type == ReviewerType.Optional)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new ReviewerMadeOptionalEvent(command.ReviewerId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerTypeChanged(reviewer.ReviewerId, ReviewerType.Optional) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> MakeReviewerRequired(MakeReviewerRequiredCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var reviewer = state.Reviewers.FirstOrDefault(reviewer => reviewer.ReviewerId == command.ReviewerId);

        if (reviewer == default)
        {
            return new ReviewerNotFoundError(command.ReviewerId);
        }

        if (reviewer.Type == ReviewerType.Required)
        {
            return Array.Empty<IPullRequestEvent>();
        }

        return new ReviewerMadeRequiredEvent(command.ReviewerId);
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> Reject(RejectCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();

        if (state.Reviewers.All(reviewer => reviewer.ReviewerId != command.ReviewerId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.ReviewerId));
        }

        events.Add(new RejectedEvent(command.ReviewerId));

        return events.ToArray();
    }

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> RemoveReviewer(RemoveReviewerCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.All(reviewer => reviewer.ReviewerId != command.ReviewerId))
        {
            return Array.Empty<IPullRequestEvent>();
        }

        var events = new List<IPullRequestEvent> { new ReviewerRemovedEvent(command.ReviewerId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerRemoved(command.ReviewerId) }))
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> ResetFeedback(ResetFeedbackCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        if (state.Reviewers.All(reviewer => reviewer.ReviewerId != command.ReviewerId))
        {
            return new ReviewerNotFoundError(command.ReviewerId);
        }

        var events = new List<IPullRequestEvent> { new FeedbackResetEvent(command.ReviewerId) };

        if (ShouldAutoComplete(state with { Reviewers = state.Reviewers.WithReviewerFeedbackChanged(command.ReviewerId, ReviewerFeedback.None) }))
        {
            events.Add(new CompletedEvent());
        }

        return events.ToArray();
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

    private static CommandProcessorResult<IPullRequestEvent, IPullRequestError> WaitForAuthor(WaitForAuthorCommand command, ExistingPullRequestState state)
    {
        if (state.Status != PullRequestStatus.Active)
        {
            return new NotActiveError();
        }

        var events = new List<IPullRequestEvent>();

        if (state.Reviewers.All(reviewer => reviewer.ReviewerId != command.ReviewerId))
        {
            events.Add(new OptionalReviewerAddedEvent(command.ReviewerId));
        }

        events.Add(new WaitingForAuthorEvent(command.ReviewerId));

        return events.ToArray();
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
