// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain;
using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void Create_AlreadyCreated_ReturnsAlreadyCreatedError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        var command = new CreateCommand(Guid.NewGuid(), sourceBranch!, targetBranch!, title!);
        var state = GetPullRequestState();

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<AlreadyCreatedError>(error);
    }

    [Fact]
    public void Create_ReturnsCreatedEvent()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        var command = new CreateCommand(Guid.NewGuid(), sourceBranch!, targetBranch!, title!);
        var state = IPullRequestState.InitialState;

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        var createdEvent = Assert.IsType<CreatedEvent>(@event);
        Assert.Equal(command.Owner, createdEvent.Owner);
        Assert.Equal(command.SourceBranch, createdEvent.SourceBranch);
        Assert.Equal(command.TargetBranch, createdEvent.TargetBranch);
        Assert.Equal(command.Title, createdEvent.Title);
    }
}
