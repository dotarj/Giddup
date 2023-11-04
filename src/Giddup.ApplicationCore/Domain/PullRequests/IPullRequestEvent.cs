// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestEvent
{
}

public record AbandonedEvent : IPullRequestEvent;

public record ApprovedEvent(Guid UserId) : IPullRequestEvent;

public record ApprovedWithSuggestionsEvent(Guid UserId) : IPullRequestEvent;

public record AutoCompleteSetEvent : IPullRequestEvent;

public record AutoCompleteCancelledEvent : IPullRequestEvent;

public record CompletedEvent : IPullRequestEvent;

public record CreatedEvent(Guid Owner, BranchName SourceBranch, BranchName TargetBranch, Title Title) : IPullRequestEvent;

public record DescriptionChangedEvent(string Description) : IPullRequestEvent;

public record FeedbackResetEvent(Guid UserId) : IPullRequestEvent;

public record OptionalReviewerAddedEvent(Guid UserId) : IPullRequestEvent;

public record ReactivatedEvent : IPullRequestEvent;

public record RejectedEvent(Guid UserId) : IPullRequestEvent;

public record RequiredReviewerAddedEvent(Guid UserId) : IPullRequestEvent;

public record ReviewerMadeOptionalEvent(Guid UserId) : IPullRequestEvent;

public record ReviewerMadeRequiredEvent(Guid UserId) : IPullRequestEvent;

public record ReviewerRemovedEvent(Guid UserId) : IPullRequestEvent;

public record TargetBranchChangedEvent(BranchName TargetBranch) : IPullRequestEvent;

public record TitleChangedEvent(Title Title) : IPullRequestEvent;

public record WaitingForAuthorEvent(Guid UserId) : IPullRequestEvent;

public record WorkItemLinkedEvent(Guid WorkItemId) : IPullRequestEvent;

public record WorkItemRemovedEvent(Guid WorkItemId) : IPullRequestEvent;
