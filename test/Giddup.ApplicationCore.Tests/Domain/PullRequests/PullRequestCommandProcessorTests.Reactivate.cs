// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task Reactivate_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new ReactivateCommand();
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotCreatedError>(error);
    }

    [Fact]
    public async Task Reactivate_AlreadyActive_ReturnsNoEvents()
    {
        // Arrange
        var command = new ReactivateCommand();
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task Reactivate_Completed_ReturnsNotAbandonedError()
    {
        // Arrange
        var command = new ReactivateCommand();
        var state = GetPullRequestState(status: PullRequestStatus.Completed);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotAbandonedError>(error);
    }

    [Fact]
    public async Task Reactivate_ReturnsReactivatedEvent()
    {
        // Arrange
        var command = new ReactivateCommand();
        var state = GetPullRequestState(status: PullRequestStatus.Abandoned);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<ReactivatedEvent>(@event);
    }
}
