// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

[Authorize]
[ApiController]
public class PullRequestQueriesController : ControllerBase
{
    private const string GraphQLQueryType = "pullRequests";

    private readonly GraphQLQueryExecutor _graphQLQueryExecutor;

    public PullRequestQueriesController(GraphQLQueryExecutor graphQLQueryExecutor)
        => _graphQLQueryExecutor = graphQLQueryExecutor;

    [HttpGet]
    [Route("/pull-requests")]
    [ProducesResponseType(typeof(ListResult<PullRequest>), 200)]
    public async Task<IActionResult> Get([Required] string fields = "", int skip = 0, int take = 10, string order = "", Guid? createdBy = null, PullRequestStatus? status = null, string? targetBranch = null)
    {
        var queryBuilder = GraphQLQueryBuilder<PullRequest>.Create(GraphQLQueryType, fields)
            .WithOffsetPaging(skip, take)
            .WithSorting(order)
            .WithWhereEquals(pullRequest => pullRequest.CreatedBy.Id, createdBy)
            .WithWhereEquals(pullRequest => pullRequest.Status, status)
            .WithWhereEquals(pullRequest => pullRequest.TargetBranch, targetBranch);

        return await _graphQLQueryExecutor.ExecuteList(queryBuilder);
    }

    [HttpGet]
    [Route("/pull-requests/{pullRequestId:guid}")]
    [ProducesResponseType(typeof(PullRequest), 200)]
    public Task<IActionResult> Get(Guid pullRequestId, [Required] string fields = "")
    {
        var queryBuilder = GraphQLQueryBuilder<PullRequest>.Create(GraphQLQueryType, fields)
            .WithWhereEquals(pullRequest => pullRequest.Id, pullRequestId);

        return _graphQLQueryExecutor.ExecuteSingle(queryBuilder);
    }
}
