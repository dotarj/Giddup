// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using OneOf;

namespace Giddup.Domain.PullRequests;

public class Title : ValueObject
{
    private readonly string _value;

    private Title(string value) => _value = value;

    public static OneOf<ITitleError, Title> Create(string value)
    {
        if (value.All(char.IsWhiteSpace))
        {
            return new TitleEmptyOrWhitespaceError();
        }

        return new Title(value);
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
