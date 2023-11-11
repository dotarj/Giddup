// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Giddup.ApplicationCore.Domain;
using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public class PullRequestStateTests
{
    [Fact]
    public void ProcessEvent_Created_ReturnsPullRequestexistingState()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch);
        _ = Title.TryCreate("baz", out var title);
        var @event = new CreatedEvent(Guid.NewGuid(), sourceBranch!, targetBranch!, title!);
        var state = IPullRequestState.InitialState;

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(@event.OwnerId, existingState.OwnerId);
        Assert.Equal(@event.SourceBranch, existingState.SourceBranch);
        Assert.Equal(@event.TargetBranch, existingState.TargetBranch);
        Assert.Equal(@event.Title, existingState.Title);
    }

    [Fact]
    public void ProcessEvent_TitleChanged_AddsRequiredReviewer()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title);
        var @event = new TitleChangedEvent(title!);
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(@event.Title, existingState.Title);
    }

    [Fact]
    public void ProcessEvent_DescriptionChanged_AddsRequiredReviewer()
    {
        // Arrange
        var @event = new DescriptionChangedEvent("baz");
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(@event.Description, existingState.Description);
    }

    [Fact]
    public void ProcessEvent_RequiredReviewerAdded_AddsRequiredReviewer()
    {
        // Arrange
        var @event = new RequiredReviewerAddedEvent(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        var reviewer = Assert.Single(existingState.Reviewers);
        Assert.Equal(@event.ReviewerId, reviewer.ReviewerId);
        Assert.Equal(ReviewerType.Required, reviewer.Type);
        Assert.Equal(ReviewerFeedback.None, reviewer.Feedback);
    }

    [Fact]
    public void ProcessEvent_OptionalReviewerAdded_AddsOptionalReviewer()
    {
        // Arrange
        var @event = new OptionalReviewerAddedEvent(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        var reviewer = Assert.Single(existingState.Reviewers);
        Assert.Equal(@event.ReviewerId, reviewer.ReviewerId);
        Assert.Equal(ReviewerType.Optional, reviewer.Type);
        Assert.Equal(ReviewerFeedback.None, reviewer.Feedback);
    }

    [Fact]
    public void ProcessEvent_ReviewerMadeRequired_MakesReviewerRequired()
    {
        // Arrange
        var @event = new ReviewerMadeRequiredEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerType.Required, existingState.Reviewers.First().Type);
    }

    [Fact]
    public void ProcessEvent_ReviewerMadeOptional_MakesReviewerOptional()
    {
        // Arrange
        var @event = new ReviewerMadeOptionalEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerType.Optional, existingState.Reviewers.First().Type);
    }

    [Fact]
    public void ProcessEvent_ReviewerRemoved_RemovesReviewer()
    {
        // Arrange
        var @event = new ReviewerRemovedEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Empty(existingState.Reviewers);
    }

    [Fact]
    public void ProcessEvent_Approved_ChangesFeedbackToApproved()
    {
        // Arrange
        var @event = new ApprovedEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerFeedback.Approved, existingState.Reviewers.First().Feedback);
    }

    [Fact]
    public void ProcessEvent_ApprovedWithSuggestions_ChangesFeedbackToApprovedWithSuggestions()
    {
        // Arrange
        var @event = new ApprovedWithSuggestionsEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerFeedback.ApprovedWithSuggestions, existingState.Reviewers.First().Feedback);
    }

    [Fact]
    public void ProcessEvent_WaitingForAuthor_ChangesFeedbackToWaitingForAuthor()
    {
        // Arrange
        var @event = new WaitingForAuthorEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerFeedback.WaitingForAuthor, existingState.Reviewers.First().Feedback);
    }

    [Fact]
    public void ProcessEvent_Rejected_ChangesFeedbackToRejected()
    {
        // Arrange
        var @event = new RejectedEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerFeedback.Rejected, existingState.Reviewers.First().Feedback);
    }

    [Fact]
    public void ProcessEvent_FeedbackReset_ResetsFeedbackToNone()
    {
        // Arrange
        var @event = new FeedbackResetEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.ReviewerId, ReviewerType.Required, ReviewerFeedback.Approved)));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(ReviewerFeedback.None, existingState.Reviewers.First().Feedback);
    }

    [Fact]
    public void ProcessEvent_WorkItemLinked_AddsWorkItem()
    {
        // Arrange
        var @event = new WorkItemLinkedEvent(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        var workItemId = Assert.Single(existingState.WorkItems);
        Assert.Equal(@event.WorkItemId, workItemId);
    }

    [Fact]
    public void ProcessEvent_WorkItemRemoved_AddsWorkItem()
    {
        // Arrange
        var @event = new WorkItemRemovedEvent(Guid.NewGuid());
        var state = GetPullRequestState(workItems: GetWorkItems(@event.WorkItemId));

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Empty(existingState.WorkItems);
    }

    [Fact]
    public void ProcessEvent_Completed_ChangesStatusToCompleted()
    {
        // Arrange
        var @event = new CompletedEvent();
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(PullRequestStatus.Completed, existingState.Status);
    }

    [Fact]
    public void ProcessEvent_AutoCompleteSet_SetsAutoCompleteModeToEnabled()
    {
        // Arrange
        var @event = new AutoCompleteSetEvent();
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(AutoCompleteMode.Enabled, existingState.AutoCompleteMode);
    }

    [Fact]
    public void ProcessEvent_AutoCompleteSet_SetsAutoCompleteModeToDisabled()
    {
        // Arrange
        var @event = new AutoCompleteCancelledEvent();
        var state = GetPullRequestState(autoCompleteMode: AutoCompleteMode.Enabled);

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(AutoCompleteMode.Disabled, existingState.AutoCompleteMode);
    }

    [Fact]
    public void ProcessEvent_Abandon_ChangesStatusToAbandoned()
    {
        // Arrange
        var @event = new AbandonedEvent();
        var state = GetPullRequestState();

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(PullRequestStatus.Abandoned, existingState.Status);
    }

    [Fact]
    public void ProcessEvent_Reactivate_ChangesStatusToActive()
    {
        // Arrange
        var @event = new ReactivatedEvent();
        var state = GetPullRequestState(status: PullRequestStatus.Abandoned);

        // Act
        var result = IPullRequestState.ProcessEvent(state, @event);

        // Assert
        var existingState = Assert.IsType<ExistingPullRequestState>(result);
        Assert.Equal(PullRequestStatus.Active, existingState.Status);
    }

    private static IPullRequestState GetPullRequestState(Guid? owner = null, BranchName? sourceBranch = null, BranchName? targetBranch = null, Title? title = null, string? description = null, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode = CheckForLinkedWorkItemsMode.Disabled, AutoCompleteMode autoCompleteMode = AutoCompleteMode.Disabled, PullRequestStatus status = PullRequestStatus.Active, ImmutableList<(Guid ReviewerId, ReviewerType Type, ReviewerFeedback Feedback)>? reviewers = null, ImmutableList<Guid>? workItems = null)
    {
        if (title is null)
        {
            _ = Title.TryCreate("title", out title!);
        }

        if (sourceBranch is null)
        {
            _ = BranchName.TryCreate("refs/heads/source", out sourceBranch!);
        }

        if (targetBranch is null)
        {
            _ = BranchName.TryCreate("refs/heads/target", out targetBranch!);
        }

        return new ExistingPullRequestState(owner ?? Guid.NewGuid(), sourceBranch, targetBranch, title, description ?? "description", checkForLinkedWorkItemsMode, autoCompleteMode, status, reviewers ?? GetReviewers(), workItems ?? GetWorkItems());
    }

    private static ImmutableList<(Guid ReviewerId, ReviewerType Type, ReviewerFeedback Feedback)> GetReviewers(params (Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)[] reviewers)
        => reviewers
            .ToImmutableList();

    private static ImmutableList<Guid> GetWorkItems(params Guid[] workItems)
        => workItems
            .ToImmutableList();
}
