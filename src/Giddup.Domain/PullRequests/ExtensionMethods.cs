// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Domain.PullRequests;

public static class ExtensionMethods
{
    internal static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerAdded(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerType type)
        => reviewers
            .Append((userId, type, ReviewerFeedback.None))
            .ToList()
            .AsReadOnly();

    internal static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerFeedbackChanged(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerFeedback feedback)
        => reviewers
            .Select(reviewer => reviewer.UserId == userId ? new(reviewer.UserId, reviewer.Type, feedback) : reviewer)
            .ToList()
            .AsReadOnly();

    internal static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerRemoved(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId)
        => reviewers
            .Where(reviewer => reviewer.UserId != userId)
            .ToList()
            .AsReadOnly();

    internal static IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerTypeChanged(this IReadOnlyCollection<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerType type)
        => reviewers
            .Select(reviewer => reviewer.UserId.Equals(userId) ? new(reviewer.UserId, type, reviewer.Feedback) : reviewer)
            .ToList()
            .AsReadOnly();

    internal static IReadOnlyCollection<Guid> WithWorkItemLinked(this IReadOnlyCollection<Guid> workItems, Guid workItemId)
        => workItems
            .Append(workItemId)
            .ToList()
            .AsReadOnly();

    internal static IReadOnlyCollection<Guid> WithWorkItemRemoved(this IReadOnlyCollection<Guid> workItems, Guid workItemId)
        => workItems
            .Where(existingWorkItemId => existingWorkItemId != workItemId)
            .ToList()
            .AsReadOnly();
}
