// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Application.PullRequests;
using Giddup.Domain.PullRequests;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

[ApiController]
public class PullRequestsController : ControllerBase
{
    private readonly IPullRequestService _pullRequestService;

    public PullRequestsController(IPullRequestService pullRequestService) => _pullRequestService = pullRequestService;

    [HttpPost]
    [Route("/pull-requests/create")]
    public async Task<IActionResult> Create(CreateCommand command)
    {
        var pullRequestId = Guid.NewGuid();

        var error = await _pullRequestService.Execute(pullRequestId, command);

        return PullRequestsPresenter.Present(error, Request.Path, () => Created($"/pull-requests/{pullRequestId}", null));
    }

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/change-title")]
    public Task<IActionResult> ChangeTitle(Guid pullRequestId, ChangeTitleCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/change-description")]
    public Task<IActionResult> ChangeDescription(Guid pullRequestId, ChangeDescriptionCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/add-required-reviewer")]
    public Task<IActionResult> AddRequiredReviewer(Guid pullRequestId, AddRequiredReviewerCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/add-optional-reviewer")]
    public Task<IActionResult> AddOptionalReviewer(Guid pullRequestId, AddOptionalReviewerCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/make-reviewer-required")]
    public Task<IActionResult> MakeReviewerRequired(Guid pullRequestId, MakeReviewerRequiredCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/make-reviewer-optional")]
    public Task<IActionResult> MakeReviewerOptional(Guid pullRequestId, MakeReviewerOptionalCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/remove-reviewer")]
    public Task<IActionResult> Remove(Guid pullRequestId, RemoveReviewerCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/approve")]
    public Task<IActionResult> Approve(Guid pullRequestId, ApproveCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/approve-with-suggestions")]
    public Task<IActionResult> ApproveWithSuggestions(Guid pullRequestId, ApproveWithSuggestionsCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/wait-for-author")]
    public Task<IActionResult> WaitForAuthor(Guid pullRequestId, WaitForAuthorCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/reject")]
    public Task<IActionResult> Reject(Guid pullRequestId, RejectCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/reset-feedback")]
    public Task<IActionResult> ResetFeedback(Guid pullRequestId, ResetFeedbackCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/link-work-item")]
    public Task<IActionResult> LinkWorkItem(Guid pullRequestId, LinkWorkItemCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/remove-work-item")]
    public Task<IActionResult> RemoveWorkItem(Guid pullRequestId, RemoveWorkItemCommand command) => ExecuteAndPresent(pullRequestId, command);

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/complete")]
    public Task<IActionResult> Complete(Guid pullRequestId) => ExecuteAndPresent(pullRequestId, new CompleteCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/set-auto-complete")]
    public Task<IActionResult> SetAutoComplete(Guid pullRequestId) => ExecuteAndPresent(pullRequestId, new SetAutoCompleteCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/cancel-auto-complete")]
    public Task<IActionResult> CancelAutoComplete(Guid pullRequestId) => ExecuteAndPresent(pullRequestId, new CancelAutoCompleteCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/abandon")]
    public Task<IActionResult> Abandon(Guid pullRequestId) => ExecuteAndPresent(pullRequestId, new AbandonCommand());

    [HttpPost]
    [Route("/pull-requests/{pullRequestId:guid}/reactivate")]
    public Task<IActionResult> Reactivate(Guid pullRequestId) => ExecuteAndPresent(pullRequestId, new ReactivateCommand());

    private async Task<IActionResult> ExecuteAndPresent(Guid pullRequestId, IPullRequestCommand command)
    {
        var error = await _pullRequestService.Execute(pullRequestId, command);

        return PullRequestsPresenter.Present(error, Request.Path);
    }
}
