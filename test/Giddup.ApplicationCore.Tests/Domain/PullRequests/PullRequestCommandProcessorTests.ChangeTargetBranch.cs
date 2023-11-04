// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain;
using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task ChangeTargetBranch_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new ChangeTargetBranchCommand(targetBranch!, IsExistingBranch);
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
    public async Task ChangeTargetBranch_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new ChangeTargetBranchCommand(targetBranch!, IsExistingBranch);
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task ChangeTargetBranch_SameTargetBranch_ReturnsNoEvents()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new ChangeTargetBranchCommand(targetBranch!, IsExistingBranch);
        var state = GetPullRequestState(targetBranch: command.TargetBranch);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Fact]
    public async Task ChangeTargetBranch_InvalidTargetBranch_ReturnsInvalidTargetBranchError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(false);
        var command = new ChangeTargetBranchCommand(targetBranch!, IsExistingBranch);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<InvalidTargetBranchError>(error);
    }

    [Fact]
    public async Task ChangeTargetBranch_SameSourceAndTargetBranch_ReturnsTargetBranchEqualsSourceBranchError()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/foo", out var sourceBranch, out _);
        _ = BranchName.TryCreate("refs/heads/foo", out var targetBranch, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new ChangeTargetBranchCommand(targetBranch!, IsExistingBranch);
        var state = GetPullRequestState(sourceBranch: sourceBranch);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<TargetBranchEqualsSourceBranchError>(error);
    }

    [Fact]
    public async Task ChangeTargetBranch_ReturnsTargetBranchChangedEvent()
    {
        // Arrange
        _ = BranchName.TryCreate("refs/heads/bar", out var targetBranch, out _);
        Task<bool> IsExistingBranch(BranchName branch) => Task.FromResult(true);
        var command = new ChangeTargetBranchCommand(targetBranch!, IsExistingBranch);
        var state = GetPullRequestState();

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<TargetBranchChangedEvent>(@event);
    }
}
