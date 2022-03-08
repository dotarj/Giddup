// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Collections;
using Giddup.Domain.PullRequests;

namespace Giddup.Infrastructure;

public class EventMapping : IEnumerable
{
    private readonly object _addLock = new();
    private readonly Dictionary<string, Type> _typeByName = new();
    private readonly Dictionary<Type, string> _nameByType = new();

    public static EventMapping Value { get; } = new()
    {
        { "created", typeof(CreatedEvent) },
        { "title-changed", typeof(TitleChangedEvent) },
        { "description-changed", typeof(DescriptionChangedEvent) },
        { "required-reviewer-added", typeof(RequiredReviewerAddedEvent) },
        { "optional-reviewer-added", typeof(OptionalReviewerAddedEvent) },
        { "reviewer-made-required", typeof(ReviewerMadeRequiredEvent) },
        { "reviewer-made-optional", typeof(ReviewerMadeOptionalEvent) },
        { "reviewer-removed", typeof(ReviewerRemovedEvent) },
        { "approved", typeof(ApprovedEvent) },
        { "approved-with-suggestions", typeof(ApprovedWithSuggestionsEvent) },
        { "waiting-for-author", typeof(WaitingForAuthorEvent) },
        { "rejected", typeof(RejectedEvent) },
        { "feedback-reset", typeof(FeedbackResetEvent) },
        { "work-item-linked", typeof(WorkItemLinkedEvent) },
        { "work-item-removed", typeof(WorkItemRemovedEvent) },
        { "completed", typeof(CompletedEvent) },
        { "auto-complete-set", typeof(AutoCompleteSetEvent) },
        { "auto-complete-cancelled", typeof(AutoCompleteCancelledEvent) },
        { "abandoned", typeof(AbandonedEvent) },
        { "reactivated", typeof(ReactivatedEvent) },
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
