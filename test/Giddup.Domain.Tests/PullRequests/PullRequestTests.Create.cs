// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void Create_AlreadyCreated_ReturnsAlreadyCreatedError()
    {
        // Arrange
        var command = new CreateCommand(BranchName.Create("refs/heads/foo").AsT1, BranchName.Create("refs/heads/bar").AsT1, Title.Create("baz").AsT1);
        var state = GetPullRequestState();

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<AlreadyCreatedError>(error);
    }

    [Fact]
    public void Create_ReturnsCreatedEvent()
    {
        // Arrange
        var command = new CreateCommand(BranchName.Create("refs/heads/foo").AsT1, BranchName.Create("refs/heads/bar").AsT1, Title.Create("baz").AsT1);
        var state = PullRequest.InitialState;

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        var createdEvent = Assert.IsType<CreatedEvent>(@event);
        Assert.Equal(command.SourceBranch, createdEvent.SourceBranch);
        Assert.Equal(command.TargetBranch, createdEvent.TargetBranch);
        Assert.Equal(command.Title, createdEvent.Title);
    }
}
