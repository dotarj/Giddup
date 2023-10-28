// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void RemoveWorkItem_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new RemoveWorkItemCommand(Guid.NewGuid());
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
    public void RemoveWorkItem_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new RemoveWorkItemCommand(Guid.NewGuid());
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void RemoveWorkItem_ExistingWorkItem_ReturnsNoEvents()
    {
        // Arrange
        var command = new RemoveWorkItemCommand(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public void RemoveWorkItem_ReturnsWorkItemRemovedEvent()
    {
        // Arrange
        var command = new RemoveWorkItemCommand(Guid.NewGuid());
        var state = GetPullRequestState(workItems: GetWorkItems(command.WorkItemId));

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<WorkItemRemovedEvent>(@event);
    }
}
