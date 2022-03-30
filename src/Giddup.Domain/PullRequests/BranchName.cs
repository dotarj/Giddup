// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Giddup.Domain.PullRequests;

public class BranchName : ValueObject
{
    private readonly string _value;

    private BranchName(string value) => _value = value;

    public static bool TryCreate(string value, [NotNullWhen(true)]out BranchName? branchName, [NotNullWhen(false)]out IBranchNameError? error)
    {
        if (!IsValidBranchName(value))
        {
            branchName = null;
            error = new InvalidBranchNameError();

            return false;
        }

        branchName = new BranchName(value);
        error = null;

        return true;
    }

    public override string ToString() => _value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return _value;
    }

    private static bool IsValidBranchName(string value)
    {
        // Git imposes the following rules on how references are named (https://git-scm.com/docs/git-check-ref-format):
        char? previousCharacter = null;

        // 2. They must contain at least one /.
        if (!value.StartsWith("refs/heads/"))
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];

            // 1. They can include slash / for hierarchical (directory) grouping, but no slash-separated component can
            // begin with a dot . or end with the sequence .lock.
            if (character == '.' && (previousCharacter == '/' || i == 0))
            {
                return false;
            }

            if (character == '/')
            {
                if (i > 4 && value[(i - 5)..i] == ".lock")
                {
                    return false;
                }
            }

            if (i == value.Length - 1)
            {
                if (i > 3 && value[(i - 4)..(i + 1)] == ".lock")
                {
                    return false;
                }
            }

            // 3. They cannot have two consecutive dots .. anywhere.
            if (character == '.' && previousCharacter == '.')
            {
                return false;
            }

            // 4. They cannot have ASCII control characters (i.e. bytes whose values are lower than \040, or \177 DEL),
            // space, tilde ~, caret ^, or colon : anywhere.
            // 5. They cannot have question-mark ?, asterisk *, or open bracket [ anywhere.
            // 10. They cannot contain a \.
            if (character < 32 || character == 127 || char.IsWhiteSpace(character) || character is '~' or '^' or ':' or '?' or '*' or '[' or '\\')
            {
                return false;
            }

            // 6. They cannot begin or end with a slash / or contain multiple consecutive slashes.
            if (character == '/' && (i == 0 || i == value.Length - 1 || previousCharacter == '/'))
            {
                return false;
            }

            // 7. They cannot end with a dot ..
            if (character == '.' && i == value.Length - 1)
            {
                return false;
            }

            // 8. They cannot contain a sequence @{.
            if (character == '{' && previousCharacter == '@')
            {
                return false;
            }

            // 9. They cannot be the single character @.
            if (character == '@' && i == 0 && value.Length == 1)
            {
                return false;
            }

            previousCharacter = character;
        }

        return true;
    }
}

public interface IBranchNameError
{
}

public record InvalidBranchNameError : IBranchNameError;
