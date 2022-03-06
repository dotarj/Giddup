// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void LinkWorkItem_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new LinkWorkItemCommand(Guid.NewGuid());
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
    public void LinkWorkItem_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new LinkWorkItemCommand(Guid.NewGuid());
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void LinkWorkItem_ExistingWorkItem_ReturnsNoEvents()
    {
        // Arrange
        var command = new LinkWorkItemCommand(Guid.NewGuid());
        var state = GetPullRequestState(workItems: GetWorkItems(command.WorkItemId));

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Theory]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.ApprovedWithSuggestions, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.None, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Enabled)]
    [InlineData(null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled)]
    public void LinkWorkItem_ReturnsWorkItemLinkedEventAndCompletedEvent(string? reviewerUserId, ReviewerType reviewerType, ReviewerFeedback reviewerFeedback, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode)
    {
        // Arrange
        var command = new LinkWorkItemCommand(Guid.NewGuid());
        var reviewers = reviewerUserId != null ? GetReviewers((Guid.Parse(reviewerUserId), reviewerType, reviewerFeedback)) : GetReviewers();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: checkForLinkedWorkItemsMode, autoCompleteMode: AutoCompleteMode.Enabled, reviewers: reviewers);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Equal(2, events!.Count);
        _ = Assert.IsType<WorkItemLinkedEvent>(events.First());
        _ = Assert.IsType<CompletedEvent>(events.Last());
    }

    [Theory]
    [InlineData(AutoCompleteMode.Disabled, null, ReviewerType.Required, ReviewerFeedback.Approved)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.WaitingForAuthor)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.Rejected)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.None)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.WaitingForAuthor)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.Rejected)]
    public void LinkWorkItem_ReturnsWorkItemLinkedEvent(AutoCompleteMode autoCompleteMode, string? reviewerUserId, ReviewerType reviewerType, ReviewerFeedback reviewerFeedback)
    {
        // Arrange
        var command = new LinkWorkItemCommand(Guid.NewGuid());
        var reviewers = reviewerUserId != null ? GetReviewers((Guid.Parse(reviewerUserId), reviewerType, reviewerFeedback)) : GetReviewers();
        var state = GetPullRequestState(autoCompleteMode: autoCompleteMode, reviewers: reviewers);

        // Act
        var result = PullRequest.Decide(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<WorkItemLinkedEvent>(@event);
    }
}
