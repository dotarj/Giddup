// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Endpoints.PullRequests;

public static class PullRequestsPresenter
{
    public static IActionResult Present(IPullRequestError? error, string requestPath) => Present(error, requestPath, () => new OkResult());

    public static IActionResult Present(IPullRequestError? error, string requestPath, Func<IActionResult> success)
    {
        ObjectResult ProblemDetailsResult(string detail, string action) => new(new ProblemDetails
        {
            Detail = detail,
            Instance = requestPath,
            Status = 400,
            Title = "Could not create or update pull request.",
            Type = $"urn:giddup/pull-request/{action}",
        })
        {
            StatusCode = 400
        };

        if (error != null)
        {
            return error switch
            {
                NotCreatedError => new NotFoundResult(),
                ReviewerNotFoundError => ProblemDetailsResult("The given reviewer was not found.", "reviewer-not-found"),
                NotAllRequiredReviewersApprovedError => ProblemDetailsResult("Not all required reviewers approved the pull request.", "not-all-required-reviewers-approved"),
                FeedbackContainsWaitForAuthorOrRejectError => ProblemDetailsResult("Pull request blocked by one or more reviewers.", "feedback-contains-wait-for-author-or-reject"),
                NoWorkItemLinkedError => ProblemDetailsResult("At least one work item should be linked.", "no-work-item-linked"),
                NotActiveError => ProblemDetailsResult("Only active pull requests can be modified.", "not-active"),
                NotAbandonedError => ProblemDetailsResult("Only abandoned pull requests can be reactivated.", "not-abandoned"),

                _ => throw new InvalidOperationException($"Error '{error.GetType().FullName}' not supported.")
            };
        }

        return success();
    }
}
