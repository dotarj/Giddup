// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public partial class PullRequestCommandProcessorTests
{
    [Fact]
    public async Task MakeReviewerOptional_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new MakeReviewerOptionalCommand(Guid.NewGuid());
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
    public async Task MakeReviewerOptional_InvalidStatus_ReturnsNotActiveError(PullRequestStatus status)
    {
        // Arrange
        var command = new MakeReviewerOptionalCommand(Guid.NewGuid());
        var state = GetPullRequestState(status: status);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<NotActiveError>(error);
    }

    [Fact]
    public async Task MakeReviewerOptional_NotExistingReviewer_ReturnsReviewerNotFoundError()
    {
        // Arrange
        var command = new MakeReviewerOptionalCommand(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((Guid.NewGuid(), ReviewerType.Required, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.False(result.TryGetEvents(out _, out var error));
        _ = Assert.IsType<ReviewerNotFoundError>(error);
    }

    [Fact]
    public async Task MakeReviewerOptional_AlreadyOptional_ReturnsNoEvents()
    {
        // Arrange
        var command = new MakeReviewerOptionalCommand(Guid.NewGuid());
        var state = GetPullRequestState(reviewers: GetReviewers((command.ReviewerId, ReviewerType.Optional, ReviewerFeedback.None)));

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Empty(events);
    }

    [Theory]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled, null)]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Required, ReviewerFeedback.ApprovedWithSuggestions, CheckForLinkedWorkItemsMode.Disabled, null)]
    [InlineData("073cb612-720e-4813-adfc-ffe021d3db08", ReviewerType.Optional, ReviewerFeedback.None, CheckForLinkedWorkItemsMode.Disabled, null)]
    [InlineData(null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Enabled, "528705cb-0a97-4564-b2bb-f89f514b54e6")]
    [InlineData(null, ReviewerType.Required, ReviewerFeedback.Approved, CheckForLinkedWorkItemsMode.Disabled, null)]
    public async Task MakeReviewerOptional_ReturnsReviewerMadeOptionalEventAndCompletedEvent(string? reviewerUserId, ReviewerType reviewerType, ReviewerFeedback reviewerFeedback, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode, string? workItemId)
    {
        // Arrange
        var command = new MakeReviewerOptionalCommand(Guid.NewGuid());
        var reviewers = reviewerUserId != null ? GetReviewers((command.ReviewerId, ReviewerType.Required, ReviewerFeedback.None), (Guid.Parse(reviewerUserId), reviewerType, reviewerFeedback)) : GetReviewers((command.ReviewerId, ReviewerType.Required, ReviewerFeedback.None));
        var workItems = workItemId != null ? GetWorkItems(Guid.Parse(workItemId)) : GetWorkItems();
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: checkForLinkedWorkItemsMode, autoCompleteMode: AutoCompleteMode.Enabled, reviewers: reviewers, workItems: workItems);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        Assert.Equal(2, events!.Count);
        _ = Assert.IsType<ReviewerMadeOptionalEvent>(events.First());
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
    public async Task MakeReviewerOptional_ReturnsReviewerMadeOptionalEvent(AutoCompleteMode autoCompleteMode, string? reviewerUserId, ReviewerType reviewerType, ReviewerFeedback reviewerFeedback, CheckForLinkedWorkItemsMode checkForLinkedWorkItemsMode)
    {
        // Arrange
        var command = new MakeReviewerOptionalCommand(Guid.NewGuid());
        var reviewers = reviewerUserId != null ? GetReviewers((command.ReviewerId, ReviewerType.Required, ReviewerFeedback.None), (Guid.Parse(reviewerUserId), reviewerType, reviewerFeedback)) : GetReviewers((command.ReviewerId, ReviewerType.Required, ReviewerFeedback.None));
        var state = GetPullRequestState(checkForLinkedWorkItemsMode: checkForLinkedWorkItemsMode, autoCompleteMode: autoCompleteMode, reviewers: reviewers);

        // Act
        var result = await PullRequestCommandProcessor.Process(state, command);

        // Assert
        Assert.True(result.TryGetEvents(out var events, out _));
        var @event = Assert.Single(events);
        _ = Assert.IsType<ReviewerMadeOptionalEvent>(@event);
    }
}
