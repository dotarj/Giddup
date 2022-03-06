// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void Abandon_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new AbandonCommand();
        var state = PullRequest.InitialState;

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotCreatedError>(error);
    }

    [Fact]
    public void Abandon_AlreadyAbandoned_ReturnsNoEvents()
    {
        // Arrange
        var command = new AbandonCommand();
        var state = GetPullRequestState(status: PullRequestStatus.Abandoned);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public void Abandon_Completed_ReturnsNotActiveError()
    {
        // Arrange
        var command = new AbandonCommand();
        var state = GetPullRequestState(status: PullRequestStatus.Completed);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void Abandon_ReturnsAbandonedEvent()
    {
        // Arrange
        var command = new AbandonCommand();
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<AbandonedEvent>(@event);
    }
}
