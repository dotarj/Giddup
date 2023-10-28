// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public async Task MakeReviewerRequired_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new MakeReviewerRequiredCommand(Guid.NewGuid());
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotCreatedError>(error);
    }

    [Theory]
    [InlineData(PullRequestStatus.Abandoned)]
    [InlineData(PullRequestStatus.Completed)]
    public async Task MakeReviewerRequired_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new MakeReviewerRequiredCommand(Guid.NewGuid());
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task MakeReviewerRequired_NotExistingReviewer_ReturnsNoEvents()
    {
        // Arrange
        var command = new MakeReviewerRequiredCommand(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((Guid.NewGuid(), ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<ReviewerNotFoundError>(error);
    }

    [Fact]
    public async Task MakeReviewerRequired_AlreadyRequired_ReturnsReviewerAlreadyRequiredError()
    {
        // Arrange
        var command = new MakeReviewerRequiredCommand(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((command.UserId, ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task MakeReviewerRequired_ReturnsReviewerMadeRequiredEvent()
    {
        // Arrange
        var command = new MakeReviewerRequiredCommand(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((command.UserId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<ReviewerMadeRequiredEvent>(@event);
    }
}
