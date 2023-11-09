// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestCommand
{
}

public record AbandonCommand : IPullRequestCommand;

public record AddOptionalReviewerCommand(Guid ReviewerId, Func<Guid, Task<bool>> IsValidReviewer) : IPullRequestCommand;

public record AddRequiredReviewerCommand(Guid ReviewerId, Func<Guid, Task<bool>> IsValidReviewer) : IPullRequestCommand;

public record ApproveCommand(Guid ReviewerId) : IPullRequestCommand;

public record ApproveWithSuggestionsCommand(Guid ReviewerId) : IPullRequestCommand;

public record CancelAutoCompleteCommand : IPullRequestCommand;

public record ChangeDescriptionCommand(string Description) : IPullRequestCommand;

public record ChangeTargetBranchCommand(string TargetBranch, Func<BranchName, Task<bool>> IsExistingBranch) : IPullRequestCommand;

public record ChangeTitleCommand(string Title) : IPullRequestCommand;

public record CompleteCommand : IPullRequestCommand;

public record CreateCommand(Guid OwnerId, string SourceBranch, string TargetBranch, string Title, Func<BranchName, Task<bool>> IsExistingBranch) : IPullRequestCommand;

public record LinkWorkItemCommand(Guid WorkItemId) : IPullRequestCommand;

public record MakeReviewerOptionalCommand(Guid ReviewerId) : IPullRequestCommand;

public record MakeReviewerRequiredCommand(Guid ReviewerId) : IPullRequestCommand;

public record ReactivateCommand : IPullRequestCommand;

public record RejectCommand(Guid ReviewerId) : IPullRequestCommand;

public record RemoveReviewerCommand(Guid ReviewerId) : IPullRequestCommand;

public record RemoveWorkItemCommand(Guid WorkItemId) : IPullRequestCommand;

public record ResetFeedbackCommand(Guid ReviewerId) : IPullRequestCommand;

public record SetAutoCompleteCommand : IPullRequestCommand;

public record WaitForAuthorCommand(Guid ReviewerId) : IPullRequestCommand;
