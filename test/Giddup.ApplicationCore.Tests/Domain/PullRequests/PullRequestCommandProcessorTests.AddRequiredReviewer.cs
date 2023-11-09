// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task AddRequiredReviewer_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddRequiredReviewerCommand(Guid.NewGuid(), IsValidReviewer);
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
    public async Task AddRequiredReviewer_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddRequiredReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task AddRequiredReviewer_ExistingReviewer_ReturnsNoEvents()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddRequiredReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState(reviewers: GetReviewers((command.ReviewerId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task AddRequiredReviewer_InvalidReviewer_ReturnsInvalidReviewerError()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(false);
        var command = new AddRequiredReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<InvalidReviewerError>(error);
    }

    [Fact]
    public async Task AddRequiredReviewer_ReturnsRequiredReviewerAddedEvent()
    {
        // Arrange
        Task<bool> IsValidReviewer(Guid userId) => Task.FromResult(true);
        var command = new AddRequiredReviewerCommand(Guid.NewGuid(), IsValidReviewer);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<RequiredReviewerAddedEvent>(@event);
    }
}
