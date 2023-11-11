// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using HotChocolate.Execution;
using HotChocolate.Language;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Giddup.Presentation.Api.Controllers;

[Authorize]
[ApiController]
public class PullRequestQueriesController : ControllerBase
{
    private readonly RequestExecutorProxy _executor;

    public PullRequestQueriesController(RequestExecutorProxy executor)
        => _executor = executor;

    [HttpGet]
    [Route("/pull-requests")]
    public Task<IActionResult> Get()
    {
        var query = CreateQuery();

        void ConfigureRequest(IQueryRequestBuilder builder) => builder.SetQuery(CreateQuery());

        return _executor.ExecuteQuery(User, HttpContext.RequestServices, ConfigureRequest, GraphQLQueryExecutor.ToActionResult);
    }

    private static DocumentNode CreateQuery()
    {
        return Utf8GraphQLParser.Parse(@"query {
  pullRequests {
    items {
      id
      owner {
        ...UserFragment
      }
      sourceBranch
      targetBranch
      title
      description
      status
      autoCompleteMode
      checkForLinkedWorkItemsMode
      optionalReviewers {
        feedback
        user {
          ...UserFragment
        }
      }
      requiredReviewers {
        feedback
        user {
          ...UserFragment
        }
      }
      workItems {
        id
        title
      }
    }
  }
}

fragment UserFragment on User {
  id
  firstName
  lastName
}");
    }
}
