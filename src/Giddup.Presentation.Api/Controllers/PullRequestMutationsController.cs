// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

[Authorize]
[ApiController]
public class PullRequestMutationsController : ControllerBase
{
    private readonly IPullRequestService _pullRequestService;

    public PullRequestMutationsController(IPullRequestService pullRequestService)
        => _pullRequestService = pullRequestService;

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/abandon")]
    public Task<IActionResult> Abandon(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new AbandonPullRequestCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/add-optional-reviewer")]
    public Task<IActionResult> AddOptionalReviewer(Guid pullRequestId, [FromServices] IReviewerService reviewerService, AddOptionalReviewerInput input)
        => ProcessCommand(pullRequestId, new AddOptionalReviewerCommand(input.UserId, reviewerService.IsValidReviewer));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/add-required-reviewer")]
    public Task<IActionResult> AddRequiredReviewer(Guid pullRequestId, [FromServices] IReviewerService reviewerService, AddRequiredReviewerInput input)
        => ProcessCommand(pullRequestId, new AddRequiredReviewerCommand(input.UserId, reviewerService.IsValidReviewer));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/approve")]
    public Task<IActionResult> Approve(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new ApprovePullRequestCommand(User.GetUserId()));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/approve-with-suggestions")]
    public Task<IActionResult> ApproveWithSuggestions(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new ApprovePullRequestWithSuggestionsCommand(User.GetUserId()));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/cancel-auto-complete")]
    public Task<IActionResult> CancelAutoComplete(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new CancelAutoCompleteCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/change-description")]
    public Task<IActionResult> ChangeDescription(Guid pullRequestId, ChangeDescriptionInput input)
        => ProcessCommand(pullRequestId, new ChangeDescriptionCommand(input.Description));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/change-target-branch")]
    public Task<IActionResult> ChangeTargetBranch(Guid pullRequestId, [FromServices] IBranchService branchService, ChangeTargetBranchInput input)
        => ProcessCommand(pullRequestId, new ChangeTargetBranchCommand(input.TargetBranch, branchService.IsExistingBranch));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/change-title")]
    public Task<IActionResult> ChangeTitle(Guid pullRequestId, ChangeTitleInput input)
        => ProcessCommand(pullRequestId, new ChangeTitleCommand(input.Title));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/complete")]
    public Task<IActionResult> Complete(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new CompletePullRequestCommand());

    [HttpPost]
    [Route("/pull-requests/create")]
    public async Task<IActionResult> Create([FromServices] IBranchService branchService, CreateInput input)
    {
        var pullRequestId = Guid.NewGuid();

        var error = await _pullRequestService.ProcessCommand(pullRequestId, new CreatePullRequestCommand(DateTime.UtcNow, User.GetUserId(), input.SourceBranch, input.TargetBranch, input.Title, branchService.IsExistingBranch));

        return CreateResult(error, Request.Path, () => Created($"/pull-requests/{pullRequestId}", null));
    }

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/link-work-item")]
    public Task<IActionResult> LinkWorkItem(Guid pullRequestId, LinkWorkItemInput input)
        => ProcessCommand(pullRequestId, new LinkWorkItemCommand(input.WorkItemId));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/make-reviewer-optional")]
    public Task<IActionResult> MakeReviewerOptional(Guid pullRequestId, MakeReviewerOptionalInput input)
        => ProcessCommand(pullRequestId, new MakeReviewerOptionalCommand(input.UserId));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/make-reviewer-required")]
    public Task<IActionResult> MakeReviewerRequired(Guid pullRequestId, MakeReviewerRequiredInput input)
        => ProcessCommand(pullRequestId, new MakeReviewerRequiredCommand(input.UserId));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/reactivate")]
    public Task<IActionResult> Reactivate(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new ReactivatePullRequestCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/reject")]
    public Task<IActionResult> Reject(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new RejectPullRequestCommand(User.GetUserId()));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/remove-reviewer")]
    public Task<IActionResult> RemoveReviewer(Guid pullRequestId, RemoveReviewerInput input)
        => ProcessCommand(pullRequestId, new RemoveReviewerCommand(input.UserId));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/remove-work-item")]
    public Task<IActionResult> RemoveWorkItem(Guid pullRequestId, RemoveWorkItemInput input)
        => ProcessCommand(pullRequestId, new RemoveWorkItemCommand(input.WorkItemId));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/reset-feedback")]
    public Task<IActionResult> ResetFeedback(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new ResetFeedbackCommand(User.GetUserId()));

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/set-auto-complete")]
    public Task<IActionResult> SetAutoComplete(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new SetAutoCompleteCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/wait-for-author")]
    public Task<IActionResult> WaitForAuthor(Guid pullRequestId)
        => ProcessCommand(pullRequestId, new WaitForAuthorCommand(User.GetUserId()));

    private static IActionResult CreateResult(IPullRequestError? error, string requestPath, Func<IActionResult> success)
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
                AlreadyExistsError => ProblemDetailsResult("A Pull request with the same ID already exists.", "already-exists"),
                FeedbackContainsWaitForAuthorOrRejectError => ProblemDetailsResult("Pull request blocked by one or more reviewers.", "feedback-contains-wait-for-author-or-reject"),
                InvalidReviewerError => ProblemDetailsResult("The given reviewer is invalid.", "invalid-reviewer"),
                InvalidBranchNameError => ProblemDetailsResult("The given branch name is invalid.", "invalid-branch-name"),
                InvalidSourceBranchError => ProblemDetailsResult("The given source branch is invalid.", "invalid-source-branch"),
                InvalidTargetBranchError => ProblemDetailsResult("The given target branch is invalid.", "invalid-target-branch"),
                NotAbandonedError => ProblemDetailsResult("Only abandoned pull requests can be reactivated.", "not-abandoned"),
                NotActiveError => ProblemDetailsResult("Only active pull requests can be modified.", "not-active"),
                NotAllRequiredReviewersApprovedError => ProblemDetailsResult("Not all required reviewers approved the pull request.", "not-all-required-reviewers-approved"),
                NotFoundError => new NotFoundResult(),
                NoWorkItemLinkedError => ProblemDetailsResult("At least one work item should be linked.", "no-work-item-linked"),
                ReviewerNotFoundError => ProblemDetailsResult("The given reviewer was not found.", "reviewer-not-found"),
                TargetBranchEqualsSourceBranchError => ProblemDetailsResult("The given target branch is equal to the source branch.", "target-branch-equals-source-branch"),

                _ => throw new InvalidOperationException($"Error '{error.GetType().FullName}' not supported.")
            };
        }

        return success();
    }

    private async Task<IActionResult> ProcessCommand(Guid pullRequestId, IPullRequestCommand command)
    {
        var error = await _pullRequestService.ProcessCommand(pullRequestId, command);

        return CreateResult(error, Request.Path, () => new OkResult());
    }

    public record AddOptionalReviewerInput(Guid UserId);

    public record AddRequiredReviewerInput(Guid UserId);

    public record ChangeTargetBranchInput(string TargetBranch);

    public record ChangeTitleInput(string Title);

    public record ChangeDescriptionInput(string Description);

    public record CreateInput(string SourceBranch, string TargetBranch, string Title);

    public record MakeReviewerRequiredInput(Guid UserId);

    public record MakeReviewerOptionalInput(Guid UserId);

    public record RemoveReviewerInput(Guid UserId);

    public record LinkWorkItemInput(Guid WorkItemId);

    public record RemoveWorkItemInput(Guid WorkItemId);
}
