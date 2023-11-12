// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
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
    [ProducesResponseType(typeof(ListResult<PullRequest>), 200)]
    public Task<IActionResult> Get(string fields = "", int skip = 0, int take = 10, string order = "", int? createdBy = null, PullRequestStatus? status = null, string? targetBranch = null)
    {
        var where = string.Empty;

        if (createdBy.HasValue)
        {
            where += $"createdBy: {{ id: {{ eq: \"{createdBy.Value}\" }} }} ";
        }

        if (status.HasValue)
        {
            where += $"status: {{ eq: \"{status.Value}\" }} ";
        }

        if (targetBranch != null)
        {
            where += $"targetBranch: {{ eq: \"{targetBranch}\" }} ";
        }

        return _graphQLQueryExecutor.Execute("pullRequests", fields, skip, take, order, where);
    }

    [HttpGet]
    [Route("/pull-requests/{pullRequestId:guid}")]
    [ProducesResponseType(typeof(PullRequest), 200)]
    public Task<IActionResult> Get(Guid pullRequestId, string fields = "")
    {
        var where = $"id: {{ eq: \"{pullRequestId}\" }} ";

        return _graphQLQueryExecutor.Execute("pullRequests", fields, where);
    }
}
