// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Giddup.ApplicationCore.Domain;

public readonly struct DeciderResult<TEvent, TError>
{
    private readonly bool _isError;
    private readonly IReadOnlyCollection<TEvent>? _events;
    private readonly TError? _error;

    private DeciderResult(IEnumerable<TEvent> events)
    {
        _isError = false;
        _events = events.ToList().AsReadOnly();
        _error = default;
    }

    private DeciderResult(TError error)
    {
        _isError = true;
        _events = null;
        _error = error;
    }

    public static implicit operator DeciderResult<TEvent, TError>(TEvent @event) => new(new[] { @event });

    public static implicit operator DeciderResult<TEvent, TError>(TEvent[] events) => new(events);

    public static implicit operator DeciderResult<TEvent, TError>(TError error) => new(error);

    public bool TryGetEvents([NotNullWhen(true)]out IReadOnlyCollection<TEvent>? events, [NotNullWhen(false)]out TError? error)
    {
        events = !_isError ? _events : null;
        error = _isError ? _error : default;

        return !_isError;
    }
}
