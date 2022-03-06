// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Domain.PullRequests;

public interface IPullRequestError
{
}

public record NotCreatedError : IPullRequestError;

public record AlreadyCreatedError : IPullRequestError;

public record NotActiveError : IPullRequestError;

public record ReviewerNotFoundError(Guid UserId) : IPullRequestError;

public record NotAllRequiredReviewersApprovedError : IPullRequestError;

public record FeedbackContainsWaitForAuthorOrRejectError : IPullRequestError;

public record NoWorkItemLinkedError : IPullRequestError;

public record NotAbandonedError : IPullRequestError;
