// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using HotChocolate.Execution;
using HotChocolate.Language;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

public class GraphQLQueryExecutor
{
    private readonly RequestExecutorProxy _requestExecutorProxy;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GraphQLQueryExecutor(RequestExecutorProxy requestExecutorProxy, IHttpContextAccessor httpContextAccessor)
    {
        _requestExecutorProxy = requestExecutorProxy;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IActionResult> Execute(string queryType, string fields, int skip, int take, string order)
    {
        if (!TryCreateQuery(queryType, fields, skip, take, order, out var query))
        {
            return new BadRequestResult();
        }

        void ConfigureRequest(IQueryRequestBuilder builder) => builder.SetQuery(query);

        var user = _httpContextAccessor.HttpContext!.User;
        var requestServices = _httpContextAccessor.HttpContext!.RequestServices;

        return await ExecuteQuery(_requestExecutorProxy, user, requestServices, ConfigureRequest, ToActionResult);
    }

    private static bool TryCreateQuery(string queryType, string fields, int skip, int take, string order, [NotNullWhen(true)] out DocumentNode? query)
    {
        try
        {
            query = Utf8GraphQLParser.Parse($"query {{ {queryType}(skip: {skip}, take: {take}, order: {{ {order} }}) {{ {fields} }} }}");
        }
        catch (SyntaxException)
        {
            query = null;

            return false;
        }

        return true;
    }

    private async Task<IActionResult> ExecuteQuery(RequestExecutorProxy executor, ClaimsPrincipal user, IServiceProvider serviceProvider, Action<IQueryRequestBuilder> configureRequest, Func<IQueryResult, IActionResult> toActionResult)
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

    private IActionResult ToActionResult(IQueryResult queryResult)
    {
        if (queryResult.Errors is { Count: > 0 })
        {
            var isNotAuthorized = queryResult.Errors
                .Any(error => error.Code == "AUTH_NOT_AUTHORIZED");

            if (isNotAuthorized)
            {
                return new ForbidResult();
            }

            const string fieldDoesNotExistPattern = "The field `\\w+` does not exist on the type `\\w+`\\.";

            var hasInvalidFields = queryResult.Errors
                .Any(error => Regex.Match(error.Message, fieldDoesNotExistPattern).Success);

            if (hasInvalidFields)
            {
                return new BadRequestResult();
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
