// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Giddup.ApplicationCore.Domain.PullRequests;

public class Title : ValueObject
{
    private readonly string _value;

    private Title(string value) => _value = value;

    public static bool TryCreate(string value, [NotNullWhen(true)]out Title? title)
    {
        if (!IsValidTitle(value))
        {
            title = null;

            return false;
        }

        title = new Title(value);

        return true;
    }

    public override string ToString() => _value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return _value;
    }

    private static bool IsValidTitle(string value)
    {
        if (value.Length > 256)
        {
            return false;
        }

        if (value.All(char.IsWhiteSpace))
        {
            return false;
        }

        return true;
    }
}
