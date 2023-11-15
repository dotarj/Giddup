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
    public async Task<IActionResult> Get(string fields = "", int skip = 0, int take = 10, string order = "", Guid? createdBy = null, PullRequestStatus? status = null, string? targetBranch = null)
    {
        var queryBuilder = GraphQLQueryBuilder<PullRequest>.Create("pullRequests", fields, skip, take, order)
            .WhereEquals(pullRequest => pullRequest.CreatedBy.Id, createdBy)
            .WhereEquals(pullRequest => pullRequest.Status, status)
            .WhereEquals(pullRequest => pullRequest.TargetBranch, targetBranch);

        if (!queryBuilder.TryCreateQuery(out var query))
        {
            return new BadRequestResult();
        }

        return await _graphQLQueryExecutor.Execute(query);
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
