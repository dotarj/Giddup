// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestCommand
{
}

public record AbandonCommand : IPullRequestCommand;

public record AddOptionalReviewerCommand(Guid UserId, Func<Guid, Task<bool>> IsValidReviewer) : IPullRequestCommand;

public record AddRequiredReviewerCommand(Guid UserId, Func<Guid, Task<bool>> IsValidReviewer) : IPullRequestCommand;

public record ApproveCommand(Guid UserId) : IPullRequestCommand;

public record ApproveWithSuggestionsCommand(Guid UserId) : IPullRequestCommand;

public record CancelAutoCompleteCommand : IPullRequestCommand;

public record ChangeDescriptionCommand(string Description) : IPullRequestCommand;

public record ChangeTargetBranchCommand(BranchName TargetBranch, Func<BranchName, Task<bool>> IsExistingBranch) : IPullRequestCommand;

public record ChangeTitleCommand(Title Title) : IPullRequestCommand;

public record CompleteCommand : IPullRequestCommand;

public record CreateCommand(Guid Owner, BranchName SourceBranch, BranchName TargetBranch, Title Title, Func<BranchName, Task<bool>> IsExistingBranch) : IPullRequestCommand;

public record LinkWorkItemCommand(Guid WorkItemId) : IPullRequestCommand;

public record MakeReviewerOptionalCommand(Guid UserId) : IPullRequestCommand;

public record MakeReviewerRequiredCommand(Guid UserId) : IPullRequestCommand;

public record ReactivateCommand : IPullRequestCommand;

public record RejectCommand(Guid UserId) : IPullRequestCommand;

public record RemoveReviewerCommand(Guid UserId) : IPullRequestCommand;

public record RemoveWorkItemCommand(Guid WorkItemId) : IPullRequestCommand;

public record ResetFeedbackCommand(Guid UserId) : IPullRequestCommand;

public record SetAutoCompleteCommand : IPullRequestCommand;

public record WaitForAuthorCommand(Guid UserId) : IPullRequestCommand;
