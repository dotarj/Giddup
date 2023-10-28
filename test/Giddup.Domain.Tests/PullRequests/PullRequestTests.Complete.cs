// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void Complete_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = IPullRequestState.InitialState;

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotCreatedError>(error);
    }

    [Theory]
    [InlineData(PullRequestStatus.Abandoned)]
    [InlineData(PullRequestStatus.Completed)]
    public void Complete_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Theory]
    [InlineData(ReviewerFeedback.WaitingForAuthor)]
    [InlineData(ReviewerFeedback.Rejected)]
    public void Complete_ReviewerFeedbackWaitForAuthorOrReject_ReturnsFeedbackContainsWaitForAuthorOrRejectError(ReviewerFeedback feedback)
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(reviewers: GetReviewers((Guid.NewGuid(), ReviewerType.Optional, feedback)));

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<FeedbackContainsWaitForAuthorOrRejectError>(error);
    }

    [Fact]
    public void Complete_RequiredReviewerFeedbackNotApproveOrApproveWithSuggestions_ReturnsNotAllRequiredReviewersApprovedError()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(reviewers: GetReviewers((Guid.NewGuid(), ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotAllRequiredReviewersApprovedError>(error);
    }

    [Fact]
    public void Complete_NoWorkItemLinked_ReturnsNoWorkItemLinkedError()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: CheckForLinkedWorkItemsMode.Enabled);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NoWorkItemLinkedError>(error);
    }

    [Fact]
    public void Complete_NoWorkItemLinked_ReturnsCompletedEvent()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState();

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<CompletedEvent>(@event);
    }

    [Fact]
    public void Complete_WorkItemLinked_ReturnsCompletedEvent()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: CheckForLinkedWorkItemsMode.Enabled, workItems: GetWorkItems(Guid.NewGuid()));

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<CompletedEvent>(@event);
    }
}
