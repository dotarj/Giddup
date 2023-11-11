// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task AddOptionalReviewer_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddOptionalReviewerCommand(Guid.NewGuid(), IsValidReviewer);
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
    public async Task AddOptionalReviewer_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddOptionalReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task AddOptionalReviewer_ExistingReviewer_ReturnsNoEvents()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddOptionalReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState(reviewers: GetReviewers((command.ReviewerId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task AddOptionalReviewer_InvalidReviewer_ReturnsInvalidReviewerError()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(false);
        var command = new AddOptionalReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<InvalidReviewerError>(error);
    }

    [Fact]
    public async Task AddOptionalReviewer_ReturnsOptionalReviewerAddedEvent()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddOptionalReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<OptionalReviewerAddedEvent>(@event);
    }
}
