// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.PullRequests.QueryModel.Models;

namespace Giddup.Infrastructure.PullRequests.QueryModel;

public class PullRequestQueryProcessor : IPullRequestQueryProcessor
{
    private readonly GiddupDbContext _dbContext;

    public PullRequestQueryProcessor(GiddupDbContext dbContext)
        => _dbContext = dbContext;

    public IQueryable<PullRequest> Process()
        => _dbContext.PullRequests;
}
