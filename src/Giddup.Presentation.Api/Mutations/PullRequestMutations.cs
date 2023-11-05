// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.Services;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Mutations;

public class PullRequestMutations
{
    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> Abandon([Service] IPullRequestService pullRequestService, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new AbandonCommand());

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<InvalidReviewerError>]
    public Task<MutationResult<PullRequestMutationResult>> AddOptionalReviewer([Service] IPullRequestService pullRequestService, [FromServices] IReviewerService reviewerService, Guid pullRequestId, Guid userId)
        => ProcessCommand(pullRequestService, pullRequestId, new AddOptionalReviewerCommand(userId, reviewerService.IsValidReviewer));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<InvalidReviewerError>]
    public Task<MutationResult<PullRequestMutationResult>> AddRequiredReviewer([Service] IPullRequestService pullRequestService, [FromServices] IReviewerService reviewerService, Guid pullRequestId, Guid userId)
        => ProcessCommand(pullRequestService, pullRequestId, new AddRequiredReviewerCommand(userId, reviewerService.IsValidReviewer));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> Approve([Service] IPullRequestService pullRequestService, IResolverContext context, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new ApproveCommand(context.GetUser()!.GetUserId()));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> ApproveWithSuggestions([Service] IPullRequestService pullRequestService, IResolverContext context, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new ApproveWithSuggestionsCommand(context.GetUser()!.GetUserId()));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> CancelAutoComplete([Service] IPullRequestService pullRequestService, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new CancelAutoCompleteCommand());

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> ChangeDescription([Service] IPullRequestService pullRequestService, Guid pullRequestId, string description)
        => ProcessCommand(pullRequestService, pullRequestId, new ChangeDescriptionCommand(description));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<InvalidBranchNameError>]
    [Error<TargetBranchEqualsSourceBranchError>]
    [Error<InvalidTargetBranchError>]
    public Task<MutationResult<PullRequestMutationResult>> ChangeTargetBranch([Service] IPullRequestService pullRequestService, [FromServices] IBranchService branchService, Guid pullRequestId, string targetBranch)
        => ProcessCommand(pullRequestService, pullRequestId, new ChangeTargetBranchCommand(targetBranch, branchService.IsExistingBranch));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<InvalidTitleError>]
    public Task<MutationResult<PullRequestMutationResult>> ChangeTitle([Service] IPullRequestService pullRequestService, Guid pullRequestId, string title)
        => ProcessCommand(pullRequestService, pullRequestId, new ChangeTitleCommand(title));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<FeedbackContainsWaitForAuthorOrRejectError>]
    [Error<NotAllRequiredReviewersApprovedError>]
    [Error<NoWorkItemLinkedError>]
    public Task<MutationResult<PullRequestMutationResult>> Complete([Service] IPullRequestService pullRequestService, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new CompleteCommand());

    [Error<InvalidBranchNameError>]
    [Error<InvalidSourceBranchError>]
    [Error<InvalidTargetBranchError>]
    [Error<TargetBranchEqualsSourceBranchError>]
    [Error<InvalidTitleError>]
    public Task<MutationResult<PullRequestMutationResult>> Create([Service] IPullRequestService pullRequestService, [FromServices] IBranchService branchService, IResolverContext context, string sourceBranch, string targetBranch, string title)
    {
        var pullRequestId = Guid.NewGuid();

        return ProcessCommand(pullRequestService, pullRequestId, new CreateCommand(context.GetUser()!.GetUserId(), sourceBranch, targetBranch, title, branchService.IsExistingBranch));
    }

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> LinkWorkItem([Service] IPullRequestService pullRequestService, Guid pullRequestId, Guid workItemId)
        => ProcessCommand(pullRequestService, pullRequestId, new LinkWorkItemCommand(workItemId));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<ReviewerNotFoundError>]
    public Task<MutationResult<PullRequestMutationResult>> MakeReviewerOptional([Service] IPullRequestService pullRequestService, Guid pullRequestId, Guid userId)
        => ProcessCommand(pullRequestService, pullRequestId, new MakeReviewerOptionalCommand(userId));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<ReviewerNotFoundError>]
    public Task<MutationResult<PullRequestMutationResult>> MakeReviewerRequired([Service] IPullRequestService pullRequestService, Guid pullRequestId, Guid userId)
        => ProcessCommand(pullRequestService, pullRequestId, new MakeReviewerRequiredCommand(userId));

    [Error<NotFoundError>]
    [Error<NotAbandonedError>]
    public Task<MutationResult<PullRequestMutationResult>> Reactivate([Service] IPullRequestService pullRequestService, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new ReactivateCommand());

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> Reject([Service] IPullRequestService pullRequestService, IResolverContext context, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new RejectCommand(context.GetUser()!.GetUserId()));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> RemoveReviewer([Service] IPullRequestService pullRequestService, Guid pullRequestId, Guid userId)
        => ProcessCommand(pullRequestService, pullRequestId, new RemoveReviewerCommand(userId));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> RemoveWorkItem([Service] IPullRequestService pullRequestService, Guid pullRequestId, Guid workItemId)
        => ProcessCommand(pullRequestService, pullRequestId, new RemoveWorkItemCommand(workItemId));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    [Error<ReviewerNotFoundError>]
    public Task<MutationResult<PullRequestMutationResult>> ResetFeedback([Service] IPullRequestService pullRequestService, IResolverContext context, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new ResetFeedbackCommand(context.GetUser()!.GetUserId()));

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> SetAutoComplete([Service] IPullRequestService pullRequestService, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new SetAutoCompleteCommand());

    [Error<NotFoundError>]
    [Error<NotActiveError>]
    public Task<MutationResult<PullRequestMutationResult>> WaitForAuthor([Service] IPullRequestService pullRequestService, IResolverContext context, Guid pullRequestId)
        => ProcessCommand(pullRequestService, pullRequestId, new WaitForAuthorCommand(context.GetUser()!.GetUserId()));

    private static async Task<MutationResult<PullRequestMutationResult>> ProcessCommand(IPullRequestService pullRequestService, Guid pullRequestId, IPullRequestCommand command)
    {
        var error = await pullRequestService.ProcessCommand(pullRequestId, command);

        if (error != null)
        {
            return new(error);
        }

        return new PullRequestMutationResult(pullRequestId);
    }

    public record PullRequestMutationResult(Guid Id);
}
