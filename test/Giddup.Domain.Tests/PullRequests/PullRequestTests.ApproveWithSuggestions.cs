// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public partial class PullRequestTests
{
    [Fact]
    public void ApproveWithSuggestions_NotCreated_ReturnsNotCreatedError()
    {
        // Arrange
        var command = new ApproveWithSuggestionsCommand(Guid.NewGuid());
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
    public void ApproveWithSuggestions_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new ApproveWithSuggestionsCommand(Guid.NewGuid());
        var state = GetPullRequestState(status: status);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public void ApproveWithSuggestions_NotExistingReviewer_ReturnsOptionalReviewerAddedEventAndApprovedEvent()
    {
        // Arrange
        var command = new ApproveWithSuggestionsCommand(Guid.NewGuid());
        var state = GetPullRequestState();

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Equal(2, events!.Count);
        _ = Assert.IsType<OptionalReviewerAddedEvent>(events.First());
        _ = Assert.IsType<ApprovedWithSuggestionsEvent>(events.Last());
    }

    [Theory]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled, null)]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.ApprovedWithSuggestions, CheckForLinkedWorkItemsMode.Disabled, null)]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.None, CheckForLinkedWorkItemsMode.Disabled, null)]
    [InlineData(null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Enabled, "528705cb-0a97-4564-b2bb-f89f514b54e6")]
    [InlineData(null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled, null)]
    public void ApproveWithSuggestions_ReturnsApprovedWithSuggestionsEventAndCompletedEvent(string? reviewerUserId, ReviewerType reviewerType, ReviewerFeedback reviewerFeedback, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode, string? workItemId)
    {
        // Arrange
        var command = new ApproveWithSuggestionsCommand(Guid.NewGuid());
        var reviewers = reviewerUserId != null ? GetReviewers((command.UserId, ReviewerType.Required, ReviewerFeedback.None), (Guid.Parse(reviewerUserId), reviewerType, reviewerFeedback)) : GetReviewers((command.UserId, ReviewerType.Required, ReviewerFeedback.None));
        var workItems = workItemId != null ? GetWorkItems(Guid.Parse(workItemId)) : GetWorkItems();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: checkForLinkedWorkItemsMode, autoCompleteMode: AutoCompleteMode.Enabled, reviewers: reviewers, workItems: workItems);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Equal(2, events!.Count);
        _ = Assert.IsType<ApprovedWithSuggestionsEvent>(events.First());
        _ = Assert.IsType<CompletedEvent>(events.Last());
    }

    [Theory]
    [InlineData(AutoCompleteMode.Disabled, null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.WaitingForAuthor, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.Rejected, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.None, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.WaitingForAuthor, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(AutoCompleteMode.Enabled, "073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.Rejected, CheckForLinkedWorkItemsMode.Disabled)]
    [InlineData(AutoCompleteMode.Enabled, null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Enabled)]
    public void ApproveWithSuggestions_ReturnsApprovedWithSuggestionsEvent(AutoCompleteMode autoCompleteMode, string? reviewerUserId, ReviewerType reviewerType, ReviewerFeedback reviewerFeedback, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode)
    {
        // Arrange
        var command = new ApproveWithSuggestionsCommand(Guid.NewGuid());
        var reviewers = reviewerUserId != null ? GetReviewers((command.UserId, ReviewerType.Required, ReviewerFeedback.None), (Guid.Parse(reviewerUserId), reviewerType, reviewerFeedback)) : GetReviewers((command.UserId, ReviewerType.Required, ReviewerFeedback.None));
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: checkForLinkedWorkItemsMode, autoCompleteMode: autoCompleteMode, reviewers: reviewers);

        // Act
        var result = PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<ApprovedWithSuggestionsEvent>(@event);
    }
}
