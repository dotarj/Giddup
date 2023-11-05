// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task ChangeTitle_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title);
        var command = new ChangeTitleCommand(title!.ToString());
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
    public async Task ChangeTitle_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title);
        var command = new ChangeTitleCommand(title!.ToString());
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task ChangeTitle_SameTitle_ReturnsNoEvents()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title);
        var command = new ChangeTitleCommand(title!.ToString());
        var state = GetPullRequestState(title: title);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task ChangeTitle_ReturnsTitleChangedEvent()
    {
        // Arrange
        _ = Title.TryCreate("baz", out var title);
        var command = new ChangeTitleCommand(title!.ToString());
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<TitleChangedEvent>(@event);
    }
}
