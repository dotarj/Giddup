// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task Complete_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotFoundError>(error);
    }

    [Theory]
    [InlineData(PullRequestStatus.Abandoned)]
    [InlineData(PullRequestStatus.Completed)]
    public async Task Complete_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Theory]
    [InlineData(ReviewerFeedback.WaitingForAuthor)]
    [InlineData(ReviewerFeedback.Rejected)]
    public async Task Complete_ReviewerFeedbackWaitForAuthorOrReject_ReturnsFeedbackContainsWaitForAuthorOrRejectError(ReviewerFeedback feedback)
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(reviewers: GetReviewers((Guid.NewGuid(), ReviewerType.Optional, feedback)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<FeedbackContainsWaitForAuthorOrRejectError>(error);
    }

    [Fact]
    public async Task Complete_RequiredReviewerFeedbackNotApproveOrApproveWithSuggestions_ReturnsNotAllRequiredReviewersApprovedError()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(reviewers: GetReviewers((Guid.NewGuid(), ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotAllRequiredReviewersApprovedError>(error);
    }

    [Fact]
    public async Task Complete_NoWorkItemLinked_ReturnsNoWorkItemLinkedError()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: CheckForLinkedWorkItemsMode.Enabled);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NoWorkItemLinkedError>(error);
    }

    [Fact]
    public async Task Complete_NoWorkItemLinked_ReturnsCompletedEvent()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<CompletedEvent>(@event);
    }

    [Fact]
    public async Task Complete_WorkItemLinked_ReturnsCompletedEvent()
    {
        // Arrange
        var command = new CompleteCommand();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: CheckForLinkedWorkItemsMode.Enabled, workItems: GetWorkItems(Guid.NewGuid()));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<CompletedEvent>(@event);
    }
}
