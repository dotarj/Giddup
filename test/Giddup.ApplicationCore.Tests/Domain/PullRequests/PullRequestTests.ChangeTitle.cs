// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void ChangeTitle_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title, out _);
        var command = new ChangeTitleCommand(title!);
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
    public void ChangeTitle_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title, out _);
        var command = new ChangeTitleCommand(title!);
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void ChangeTitle_SameTitle_ReturnsNoEvents()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title, out _);
        var command = new ChangeTitleCommand(title!);
        var state = GetPullRequestState(title: command.Title);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public void ChangeTitle_ReturnsRequiredReviewerAddedEvent()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title, out _);
        var command = new ChangeTitleCommand(title!);
        var state = GetPullRequestState();

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<TitleChangedEvent>(@event);
    }
}
