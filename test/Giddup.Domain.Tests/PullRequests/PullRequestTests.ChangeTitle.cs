// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void ChangeTitle_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new ChangeTitleCommand(Title.Create("baz").AsT1);
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
    public void ChangeTitle_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new ChangeTitleCommand(Title.Create("baz").AsT1);
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void ChangeTitle_SameTitle_ReturnsNoEvents()
    {
        // Arrange
        var command = new ChangeTitleCommand(Title.Create("foo").AsT1);
        var state = GetPullRequestState(title: command.Title);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public void ChangeTitle_ReturnsRequiredReviewerAddedEvent()
    {
        // Arrange
        var command = new ChangeTitleCommand(Title.Create("baz").AsT1);
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<TitleChangedEvent>(@event);
    }
}
