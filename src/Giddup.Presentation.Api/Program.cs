// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Application.PullRequests;
using Giddup.Infrastructure.PullRequests.CommandModel;
using Giddup.Infrastructure.PullRequests.QueryModel;
using Giddup.Infrastructure.Services;
using Giddup.Presentation.Api.AppStartup;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Services
    .AddScoped<IBranchService, BranchService>()
    .AddScoped<IPullRequestEventProcessor, PullRequestEventProcessor>()
    .AddScoped<IPullRequestQueryProcessor, PullRequestQueryProcessor>()
    .AddScoped<IPullRequestService, PullRequestService>()
    .AddScoped<IPullRequestStateProvider, PullRequestStateProvider>()
    .AddScoped<IReviewerService, ReviewerService>();

_ = builder.Services
    .AddAppStartupAuthentication()
    .AddAppStartupAspNetCore()
    .AddAppStartupEntityFrameworkCore(builder.Configuration)
    .AddAppStartupGraphQL()
    .AddAppStartupSwagger();

var app = builder.Build();

_ = app
    .UseAppStartupAuthentication()
    .UseAppStartupAuthorization()
    .UseAppStartupAspNetCore()
    .UseAppStartupEntityFrameworkCore(builder.Environment.IsDevelopment())
    .UseAppStartupGraphQL()
    .UseAppStartupSwagger();

app.Run();
