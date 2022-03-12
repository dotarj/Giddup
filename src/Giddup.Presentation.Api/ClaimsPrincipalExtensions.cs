// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Security.Claims;

namespace Giddup.Presentation.Api;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            throw new InvalidOperationException();
        }

        return Guid.Parse(claim.Value);
    }
}
