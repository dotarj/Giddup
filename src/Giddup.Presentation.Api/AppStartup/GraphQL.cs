// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure;
using Giddup.Presentation.Api.Mutations;
using Giddup.Presentation.Api.Queries;

namespace Giddup.Presentation.Api.AppStartup;

public static class GraphQL
{
    public static IServiceCollection AddAppStartupGraphQL(this IServiceCollection services)
    {
        _ = services
            .AddGraphQLServer()
            .RegisterDbContext<GiddupDbContext>()
            .AddMutationType<PullRequestMutations>()
            .AddMutationConventions(applyToAllMutations: true)
            .AddQueryType<PullRequestQueries>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddType<IPullRequestError>();

        return services;
    }

    public static WebApplication UseAppStartupGraphQL(this WebApplication builder)
    {
        _ = builder.MapGraphQL();

        return builder;
    }
}
