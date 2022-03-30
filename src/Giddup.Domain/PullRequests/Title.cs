// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Giddup.Domain.PullRequests;

public class Title : ValueObject
{
    private readonly string _value;

    private Title(string value) => _value = value;

    public static bool TryCreate(string value, [NotNullWhen(true)]out Title? title, [NotNullWhen(false)]out ITitleError? error)
    {
        if (value.All(char.IsWhiteSpace))
        {
            title = null;
            error = new TitleEmptyOrWhitespaceError();

            return false;
        }

        title = new Title(value);
        error = null;

        return true;
    }

    public override string ToString() => _value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return _value;
    }
}

public interface ITitleError
{
}

public record TitleEmptyOrWhitespaceError : ITitleError;
