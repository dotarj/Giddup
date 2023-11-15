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

    public async Task<IActionResult> Execute(string queryType, string fields, string where)
    {
        if (!TryCreateQuery(queryType, fields, 0, 1, string.Empty, where, out var query))
        {
            return new BadRequestResult();
        }

        void ConfigureRequest(IQueryRequestBuilder builder) => builder.SetQuery(query);

        return await ExecuteQuery(ConfigureRequest, CreateSingleResult);
    }

    public async Task<IActionResult> Execute(string queryType, string fields, int skip, int take, string order, string where)
    {
        if (!TryCreateQuery(queryType, fields, skip, take, order, where, out var query))
        {
            return new BadRequestResult();
        }

        void ConfigureRequest(IQueryRequestBuilder builder) => builder.SetQuery(query);

        return await ExecuteQuery(ConfigureRequest, CreateListResult);
    }

    public async Task<IActionResult> Execute(DocumentNode query)
    {
        void ConfigureRequest(IQueryRequestBuilder builder) => builder.SetQuery(query);

        return await ExecuteQuery(ConfigureRequest, CreateListResult);
    }

    private static bool TryCreateQuery(string queryType, string fields, int skip, int take, string order, string where, [NotNullWhen(true)] out DocumentNode? query)
    {
        try
        {
            query = Utf8GraphQLParser.Parse($"query {{ {queryType}(skip: {skip}, take: {take}, order: {{ {order} }}, where: {{ {where} }}) {{ {fields} }} }}");
        }
        catch (SyntaxException)
        {
            query = null;

            return false;
        }

        return true;
    }

    private async Task<IActionResult> ExecuteQuery(Action<IQueryRequestBuilder> configureRequest, Func<IQueryResult, IActionResult> toActionResult)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var requestServices = _httpContextAccessor.HttpContext!.RequestServices;

        var requestBuilder = new QueryRequestBuilder();

        _ = requestBuilder
            .SetServices(requestServices)
            .SetGlobalState(nameof(ClaimsPrincipal), user);

        configureRequest(requestBuilder);

        var request = requestBuilder.Create();

        await using var result = await _requestExecutorProxy.ExecuteAsync(request);

        return toActionResult(result.ExpectQueryResult());
    }

    private IActionResult CreateSingleResult(IQueryResult queryResult)
    {
        if (queryResult.Errors is { Count: > 0 })
        {
            return CreateErrorResult(queryResult.Errors);
        }

        var rootResult = (IReadOnlyDictionary<string, object?>)queryResult.Data!.First().Value!;
        var itemsResult = (IReadOnlyList<object?>)rootResult.First().Value!;

        if (itemsResult.Count == 0)
        {
            return new NotFoundResult();
        }

        return new ContentResult
        {
            Content = JsonSerializer.Serialize(itemsResult.First()),
            ContentType = MediaTypeNames.Application.Json
        };
    }

    private IActionResult CreateListResult(IQueryResult queryResult)
    {
        if (queryResult.Errors is { Count: > 0 })
        {
            return CreateErrorResult(queryResult.Errors);
        }

        var rootResult = (IReadOnlyDictionary<string, object?>)queryResult.Data!.First().Value!;

        return new ContentResult
        {
            Content = JsonSerializer.Serialize(rootResult),
            ContentType = MediaTypeNames.Application.Json
        };
    }

    private IActionResult CreateErrorResult(IReadOnlyList<IError> errors)
    {
        var isNotAuthorized = errors.Any(error => error.Code == "AUTH_NOT_AUTHORIZED");

        if (isNotAuthorized)
        {
            return new ForbidResult();
        }

        const string fieldDoesNotExistPattern = "The field `\\w+` does not exist on the type `\\w+`\\.";

        var hasInvalidFields = errors.Any(error => Regex.Match(error.Message, fieldDoesNotExistPattern).Success);

        if (hasInvalidFields)
        {
            return new BadRequestResult();
        }

        return new StatusCodeResult(500);
    }
}
