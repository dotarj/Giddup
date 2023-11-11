// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.ApplicationCore.Domain.PullRequests;

public interface IPullRequestEvent
{
}

public record AbandonedEvent : IPullRequestEvent;

public record ApprovedEvent(Guid ReviewerId) : IPullRequestEvent;

public record ApprovedWithSuggestionsEvent(Guid ReviewerId) : IPullRequestEvent;

public record AutoCompleteSetEvent : IPullRequestEvent;

public record AutoCompleteCancelledEvent : IPullRequestEvent;

public record CompletedEvent : IPullRequestEvent;

public record CreatedEvent(DateTime CreatedAt, Guid CreatedById, BranchName SourceBranch, BranchName TargetBranch, Title Title) : IPullRequestEvent;

public record DescriptionChangedEvent(string Description) : IPullRequestEvent;

public record FeedbackResetEvent(Guid ReviewerId) : IPullRequestEvent;

public record OptionalReviewerAddedEvent(Guid ReviewerId) : IPullRequestEvent;

public record ReactivatedEvent : IPullRequestEvent;

public record RejectedEvent(Guid ReviewerId) : IPullRequestEvent;

public record RequiredReviewerAddedEvent(Guid ReviewerId) : IPullRequestEvent;

public record ReviewerMadeOptionalEvent(Guid ReviewerId) : IPullRequestEvent;

public record ReviewerMadeRequiredEvent(Guid ReviewerId) : IPullRequestEvent;

public record ReviewerRemovedEvent(Guid ReviewerId) : IPullRequestEvent;

public record TargetBranchChangedEvent(BranchName TargetBranch) : IPullRequestEvent;

public record TitleChangedEvent(Title Title) : IPullRequestEvent;

public record WaitingForAuthorEvent(Guid ReviewerId) : IPullRequestEvent;

public record WorkItemLinkedEvent(Guid WorkItemId) : IPullRequestEvent;

public record WorkItemRemovedEvent(Guid WorkItemId) : IPullRequestEvent;
