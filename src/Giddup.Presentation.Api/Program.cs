// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.Infrastructure;
using Giddup.Infrastructure.JsonConverters;
using Giddup.Infrastructure.PullRequests;
using Giddup.Infrastructure.Services;
using Giddup.Presentation.Api.AppStartup;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Services
    .AddSingleton<IBranchService, BranchService>()
    .AddSingleton<IEventStream, EventStream>()
    .AddSingleton<IPullRequestEventProcessor, PullRequestEventProcessor>()
    .AddSingleton<IPullRequestService, PullRequestService>()
    .AddSingleton<IPullRequestStateProvider, PullRequestStateProvider>()
    .AddSingleton<IReviewerService, ReviewerService>();

_ = builder.Services
    .AddAppStartupAuthentication()
    .AddAppStartupGraphQL()
    .AddAppStartupOptions(builder.Configuration)
    .AddAppStartupSwagger()
    .AddControllers();

var app = builder.Build();

_ = app
    .UseAppStartupAuthentication()
    .UseAppStartupSwagger()
    .UseAuthorization();

_ = app.MapControllers();

_ = app.MapAppStartupGraphQL();

app.Run();
