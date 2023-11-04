// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections;
using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.Infrastructure;

public class EventMapping : IEnumerable
{
    private readonly object _addLock = new();
    private readonly Dictionary<string, Type> _typeByName = new();
    private readonly Dictionary<Type, string> _nameByType = new();

    public static EventMapping Value { get; } = new()
    {
        { "abandoned", typeof(AbandonedEvent) },
        { "approved", typeof(ApprovedEvent) },
        { "approved-with-suggestions", typeof(ApprovedWithSuggestionsEvent) },
        { "auto-complete-cancelled", typeof(AutoCompleteCancelledEvent) },
        { "auto-complete-set", typeof(AutoCompleteSetEvent) },
        { "completed", typeof(CompletedEvent) },
        { "created", typeof(CreatedEvent) },
        { "description-changed", typeof(DescriptionChangedEvent) },
        { "feedback-reset", typeof(FeedbackResetEvent) },
        { "optional-reviewer-added", typeof(OptionalReviewerAddedEvent) },
        { "reactivated", typeof(ReactivatedEvent) },
        { "rejected", typeof(RejectedEvent) },
        { "required-reviewer-added", typeof(RequiredReviewerAddedEvent) },
        { "reviewer-made-optional", typeof(ReviewerMadeOptionalEvent) },
        { "reviewer-made-required", typeof(ReviewerMadeRequiredEvent) },
        { "reviewer-removed", typeof(ReviewerRemovedEvent) },
        { "target-branch-changed", typeof(TargetBranchChangedEvent) },
        { "title-changed", typeof(TitleChangedEvent) },
        { "waiting-for-author", typeof(WaitingForAuthorEvent) },
        { "work-item-linked", typeof(WorkItemLinkedEvent) },
        { "work-item-removed", typeof(WorkItemRemovedEvent) }
    };

    public void Add(string type, Type @event)
    {
        lock (_addLock)
        {
            if (_typeByName.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type '{type}' already registered.");
            }

            if (_nameByType.ContainsKey(@event))
            {
                throw new InvalidOperationException($"Event '{@event.Name}' already registered.");
            }

            _typeByName.Add(type, @event);
            _nameByType.Add(@event, type);
        }
    }

    public Type GetEventType(string eventName)
    {
        if (!_typeByName.TryGetValue(eventName, out var eventType))
        {
            throw new InvalidOperationException($"Event name '{eventName}' not registered.");
        }

        return eventType;
    }

    public string GetEventName(Type eventType)
    {
        if (!_nameByType.TryGetValue(eventType, out var eventName))
        {
            throw new InvalidOperationException($"Event type '{eventType.Name}' not registered.");
        }

        return eventName;
    }

    public IEnumerator GetEnumerator()
        => throw new NotImplementedException();
}
