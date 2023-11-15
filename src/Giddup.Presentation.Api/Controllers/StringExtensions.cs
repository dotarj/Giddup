// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Presentation.Api.Controllers;

public static class StringExtensions
{
    public static string ToLowerFirstChar(this string input)
    {
        if (input.Length == 0)
        {
            return input;
        }

        return char.ToLower(input[0]) + input[1..];
    }
}
