// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections.Immutable;

namespace Giddup.ApplicationCore.Domain.PullRequests;

public static class ExtensionMethods
{
    internal static ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerAdded(this ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerType type)
        => reviewers
            .Append((userId, type, ReviewerFeedback.None))
            .ToImmutableList();

    internal static ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerFeedbackChanged(this ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerFeedback feedback)
        => reviewers
            .Select(reviewer => reviewer.UserId == userId ? new(reviewer.UserId, reviewer.Type, feedback) : reviewer)
            .ToImmutableList();

    internal static ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerRemoved(this ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId)
        => reviewers
            .Where(reviewer => reviewer.UserId != userId)
            .ToImmutableList();

    internal static ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> WithReviewerTypeChanged(this ImmutableList<(Guid UserId, ReviewerType Type, ReviewerFeedback Feedback)> reviewers, Guid userId, ReviewerType type)
        => reviewers
            .Select(reviewer => reviewer.UserId.Equals(userId) ? new(reviewer.UserId, type, reviewer.Feedback) : reviewer)
            .ToImmutableList();

    internal static ImmutableList<Guid> WithWorkItemLinked(this ImmutableList<Guid> workItems, Guid workItemId)
        => workItems
            .Append(workItemId)
            .ToImmutableList();

    internal static ImmutableList<Guid> WithWorkItemRemoved(this ImmutableList<Guid> workItems, Guid workItemId)
        => workItems
            .Where(existingWorkItemId => existingWorkItemId != workItemId)
            .ToImmutableList();
}
