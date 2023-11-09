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
    .AddAppStartupEntityFrameworkCore(builder.Configuration)
    .AddAppStartupGraphQL()
    .AddAppStartupSwagger()
    .AddControllers();

var app = builder.Build();

_ = app
    .UseAppStartupAuthentication()
    .UseAppStartupSwagger()
    .UseAuthorization();

_ = app.UseAppStartupEntityFrameworkCore(builder.Environment.IsDevelopment());

_ = app.MapControllers();

_ = app.MapAppStartupGraphQL();

app.Run();
