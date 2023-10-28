// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain;
using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public async Task Create_AlreadyCreated_ReturnsAlreadyCreatedError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new CreateCommand(Guid.NewGuid(), sourceBranch!, targetBranch!, title!, IsExistingBranch);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<AlreadyCreatedError>(error);
    }

    [Fact]
    public async Task Create_InvalidSourceBranch_ReturnsInvalidSourceBranchError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(false);
        var command = new CreateCommand(Guid.NewGuid(), sourceBranch!, targetBranch!, title!, IsExistingBranch);
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<InvalidSourceBranchError>(error);
    }

    [Fact]
    public async Task Create_InvalidTargetBranch_ReturnsInvalidTargetBranchError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        var count = 0;
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(count++ % 2 == 0);
        var command = new CreateCommand(Guid.NewGuid(), sourceBranch!, targetBranch!, title!, IsExistingBranch);
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<InvalidTargetBranchError>(error);
    }

    [Fact]
    public async Task Create_ReturnsCreatedEvent()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        _ = Title.TryCreate("baz", out var title, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new CreateCommand(Guid.NewGuid(), sourceBranch!, targetBranch!, title!, IsExistingBranch);
        var state = IPullRequestState.InitialState;

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

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
