// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task Abandon_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new AbandonPullRequestCommand();
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotFoundError>(error);
    }

    [Fact]
    public async Task Abandon_AlreadyAbandoned_ReturnsNoEvents()
    {
        // Arrange
        var command = new AbandonPullRequestCommand();
        var state = GetPullRequestState(status: PullRequestStatus.Abandoned);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task Abandon_Completed_ReturnsNotActiveError()
    {
        // Arrange
        var command = new AbandonPullRequestCommand();
        var state = GetPullRequestState(status: PullRequestStatus.Completed);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task Abandon_ReturnsAbandonedEvent()
    {
        // Arrange
        var command = new AbandonPullRequestCommand();
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<AbandonedEvent>(@event);
    }
}
