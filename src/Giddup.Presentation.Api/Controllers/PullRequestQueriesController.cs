// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

[Authorize]
[ApiController]
public class PullRequestQueriesController : ControllerBase
{
    private readonly GraphQLQueryExecutor _graphQLQueryExecutor;

    public PullRequestQueriesController(GraphQLQueryExecutor graphQLQueryExecutor)
        => _graphQLQueryExecutor = graphQLQueryExecutor;

    [HttpGet]
    [Route("/pull-requests")]
    [ProducesResponseType(typeof(List<PullRequest>), 200)]
    public Task<IActionResult> Get(string fields = "", int skip = 0, int take = 10)
        => _graphQLQueryExecutor.Execute("pullRequests", fields, skip, take);
}
