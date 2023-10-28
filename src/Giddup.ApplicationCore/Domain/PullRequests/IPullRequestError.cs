// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestError
{
}

public record FeedbackContainsWaitForAuthorOrRejectError : IPullRequestError;

public record InvalidReviewerError : IPullRequestError;

public record InvalidSourceBranchError : IPullRequestError;

public record InvalidTargetBranchError : IPullRequestError;

public record NotAbandonedError : IPullRequestError;

public record NotActiveError : IPullRequestError;

public record NotAllRequiredReviewersApprovedError : IPullRequestError;

public record NotFoundError : IPullRequestError;

public record NoWorkItemLinkedError : IPullRequestError;

public record ReviewerNotFoundError(Guid UserId) : IPullRequestError;
