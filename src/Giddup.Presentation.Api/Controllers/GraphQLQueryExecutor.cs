// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

public static class GraphQLQueryExecutor
{
    public static async Task<IActionResult> ExecuteQuery(this RequestExecutorProxy executor, ClaimsPrincipal user, IServiceProvider serviceProvider, Action<IQueryRequestBuilder> configureRequest, Func<IQueryResult, IActionResult> toActionResult)
    {
        var requestBuilder = new QueryRequestBuilder();

        _ = requestBuilder
            .SetServices(serviceProvider)
            .SetGlobalState(nameof(ClaimsPrincipal), user);

        configureRequest(requestBuilder);

        var request = requestBuilder.Create();

        await using var result = await executor.ExecuteAsync(request);

        return toActionResult(result.ExpectQueryResult());
    }

    public static IActionResult ToActionResult(IQueryResult queryResult)
    {
        if (queryResult.Errors is { Count: > 0 })
        {
            var isNotAuthorized = queryResult.Errors
                .Any(error => error.Code == "AUTH_NOT_AUTHORIZED");

            if (isNotAuthorized)
            {
                return new ForbidResult();
            }

            return new StatusCodeResult(500);
        }

        return new ContentResult
        {
            Content = JsonSerializer.Serialize(queryResult.Data!.First().Value),
            ContentType = MediaTypeNames.Application.Json
        };
    }
}
