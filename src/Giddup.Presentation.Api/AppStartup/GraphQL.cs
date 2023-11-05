// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Presentation.Api.Mutations;
using Giddup.Presentation.Api.Queries;

// This is necessary to generate a more descriptive method name for registering GraphQL types. The default uses the
// last part of the assembly name for the method name resulting in .AddApiTypes().
[assembly: Module("GiddupTypes")]

namespace Giddup.Presentation.Api.AppStartup;

public static class GraphQL
{
    public static IServiceCollection AddAppStartupGraphQL(this IServiceCollection services)
    {
        _ = services
            .AddGraphQLServer()
            .AddMutationType<PullRequestMutations>()
            .AddMutationConventions(applyToAllMutations: true)
            .AddQueryType<PullRequestQueries>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddType<IPullRequestError>();

        return services;
    }

    public static IEndpointRouteBuilder MapAppStartupGraphQL(this IEndpointRouteBuilder builder)
    {
        _ = builder.MapGraphQL();

        return builder;
    }
}
