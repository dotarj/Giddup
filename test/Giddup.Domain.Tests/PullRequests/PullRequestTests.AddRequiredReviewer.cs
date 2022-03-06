// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void AddRequiredReviewer_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new AddRequiredReviewerCommand(Guid.NewGuid());
        var state = PullRequest.InitialState;

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotCreatedError>(error);
    }

    [Theory]
    [InlineData(PullRequestStatus.Abandoned)]
    [InlineData(PullRequestStatus.Completed)]
    public void AddRequiredReviewer_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new AddRequiredReviewerCommand(Guid.NewGuid());
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void AddRequiredReviewer_ExistingReviewer_ReturnsNoEvents()
    {
        // Arrange
        var command = new AddRequiredReviewerCommand(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((command.UserId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public void AddRequiredReviewer_ReturnsRequiredReviewerAddedEvent()
    {
        // Arrange
        var command = new AddRequiredReviewerCommand(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<RequiredReviewerAddedEvent>(@event);
    }
}
