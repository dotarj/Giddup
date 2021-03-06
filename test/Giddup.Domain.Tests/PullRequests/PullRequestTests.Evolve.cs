// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void Evolve_Created_ReturnsPullRequestCreatedState()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        var @event = new CreatedEvent(Guid.NewGuid(), sourceBranch!, targetBranch!, title!);
        var state = PullRequest.InitialState;

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(@event.Owner, createdState.Owner);
        Assert.Equal(@event.SourceBranch, createdState.SourceBranch);
        Assert.Equal(@event.TargetBranch, createdState.TargetBranch);
        Assert.Equal(@event.Title, createdState.Title);
    }

    [Fact]
    public void Evolve_TitleChanged_AddsRequiredReviewer()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title, out _);
        var @event = new TitleChangedEvent(title!);
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(@event.Title, createdState.Title);
    }

    [Fact]
    public void Evolve_DescriptionChanged_AddsRequiredReviewer()
    {
        // Arrange
        var @event = new DescriptionChangedEvent("baz");
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(@event.Description, createdState.Description);
    }

    [Fact]
    public void Evolve_RequiredReviewerAdded_AddsRequiredReviewer()
    {
        // Arrange
        var @event = new RequiredReviewerAddedEvent(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        var reviewer = Assert.Single(createdState.Reviewers);
        Assert.Equal(@event.UserId, reviewer.UserId);
        Assert.Equal(ReviewerType.Required, reviewer.Type);
        Assert.Equal(ReviewerFeedback.None, reviewer.Feedback);
    }

    [Fact]
    public void Evolve_OptionalReviewerAdded_AddsOptionalReviewer()
    {
        // Arrange
        var @event = new OptionalReviewerAddedEvent(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        var reviewer = Assert.Single(createdState.Reviewers);
        Assert.Equal(@event.UserId, reviewer.UserId);
        Assert.Equal(ReviewerType.Optional, reviewer.Type);
        Assert.Equal(ReviewerFeedback.None, reviewer.Feedback);
    }

    [Fact]
    public void Evolve_ReviewerMadeRequired_MakesReviewerRequired()
    {
        // Arrange
        var @event = new ReviewerMadeRequiredEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerType.Required, createdState.Reviewers.First().Type);
    }

    [Fact]
    public void Evolve_ReviewerMadeOptional_MakesReviewerOptional()
    {
        // Arrange
        var @event = new ReviewerMadeOptionalEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerType.Optional, createdState.Reviewers.First().Type);
    }

    [Fact]
    public void Evolve_ReviewerRemoved_RemovesReviewer()
    {
        // Arrange
        var @event = new ReviewerRemovedEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Empty(createdState.Reviewers);
    }

    [Fact]
    public void Evolve_Approved_ChangesFeedbackToApproved()
    {
        // Arrange
        var @event = new ApprovedEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerFeedback.Approved, createdState.Reviewers.First().Feedback);
    }

    [Fact]
    public void Evolve_ApprovedWithSuggestions_ChangesFeedbackToApprovedWithSuggestions()
    {
        // Arrange
        var @event = new ApprovedWithSuggestionsEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerFeedback.ApprovedWithSuggestions, createdState.Reviewers.First().Feedback);
    }

    [Fact]
    public void Evolve_WaitingForAuthor_ChangesFeedbackToWaitingForAuthor()
    {
        // Arrange
        var @event = new WaitingForAuthorEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerFeedback.WaitingForAuthor, createdState.Reviewers.First().Feedback);
    }

    [Fact]
    public void Evolve_Rejected_ChangesFeedbackToRejected()
    {
        // Arrange
        var @event = new RejectedEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerFeedback.Rejected, createdState.Reviewers.First().Feedback);
    }

    [Fact]
    public void Evolve_FeedbackReset_ResetsFeedbackToNone()
    {
        // Arrange
        var @event = new FeedbackResetEvent(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((@event.UserId, ReviewerType.Required, ReviewerFeedback.Approved)));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(ReviewerFeedback.None, createdState.Reviewers.First().Feedback);
    }

    [Fact]
    public void Evolve_WorkItemLinked_AddsWorkItem()
    {
        // Arrange
        var @event = new WorkItemLinkedEvent(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        var workItemId = Assert.Single(createdState.WorkItems);
        Assert.Equal(@event.WorkItemId, workItemId);
    }

    [Fact]
    public void Evolve_WorkItemRemoved_AddsWorkItem()
    {
        // Arrange
        var @event = new WorkItemRemovedEvent(Guid.NewGuid());
        var state = GetPullRequestState(workItems: GetWorkItems(@event.WorkItemId));

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Empty(createdState.WorkItems);
    }

    [Fact]
    public void Evolve_Completed_ChangesStatusToCompleted()
    {
        // Arrange
        var @event = new CompletedEvent();
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(PullRequestStatus.Completed, createdState.Status);
    }

    [Fact]
    public void Evolve_AutoCompleteSet_SetsAutoCompleteModeToEnabled()
    {
        // Arrange
        var @event = new AutoCompleteSetEvent();
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(AutoCompleteMode.Enabled, createdState.AutoCompleteMode);
    }

    [Fact]
    public void Evolve_AutoCompleteSet_SetsAutoCompleteModeToDisabled()
    {
        // Arrange
        var @event = new AutoCompleteCancelledEvent();
        var state = GetPullRequestState(autoCompleteMode: AutoCompleteMode.Enabled);

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(AutoCompleteMode.Disabled, createdState.AutoCompleteMode);
    }

    [Fact]
    public void Evolve_Abandon_ChangesStatusToAbandoned()
    {
        // Arrange
        var @event = new AbandonedEvent();
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(PullRequestStatus.Abandoned, createdState.Status);
    }

    [Fact]
    public void Evolve_Reactivate_ChangesStatusToActive()
    {
        // Arrange
        var @event = new ReactivatedEvent();
        var state = GetPullRequestState(status: PullRequestStatus.Abandoned);

        // Act
        var result = PullRequest.Evolve(state, @event);

        // Assert
        var createdState = Assert.IsType<PullRequestCreatedState>(result);
        Assert.Equal(PullRequestStatus.Active, createdState.Status);
    }
}
