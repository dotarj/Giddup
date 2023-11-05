// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestError
{
    string Message { get; }
}

public abstract record PullRequestError : IPullRequestError
{
    public string Message => GetType().Name;
}

public record AlreadyExistsError : PullRequestError;

public record FeedbackContainsWaitForAuthorOrRejectError : PullRequestError;

public record InvalidBranchNameError : PullRequestError;

public record InvalidReviewerError : PullRequestError;

public record InvalidSourceBranchError : PullRequestError;

public record InvalidTargetBranchError : PullRequestError;

public record InvalidTitleError : PullRequestError;

public record NotAbandonedError : PullRequestError;

public record NotActiveError : PullRequestError;

public record NotAllRequiredReviewersApprovedError : PullRequestError;

public record NotFoundError : PullRequestError;

public record NoWorkItemLinkedError : PullRequestError;

public record ReviewerNotFoundError(Guid UserId) : PullRequestError;

public record TargetBranchEqualsSourceBranchError : PullRequestError;
