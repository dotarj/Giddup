// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Application;
using Giddup.Application.PullRequests;
using Giddup.Infrastructure;
using Giddup.Infrastructure.JsonConverters;
using Giddup.Presentation.Api.AppStartup;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Services
    .AddSingleton<IPullRequestService, PullRequestService>()
    .AddSingleton<IEventStream, EventStream>();

_ = builder.Services
    .AddAppStartupAuthentication()
    .AddAppStartupOptions(builder.Configuration)
    .AddAppStartupSwagger()
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new BranchNameJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TitleJsonConverter());
    });

var app = builder.Build();

_ = app
    .UseAppStartupAuthentication()
    .UseAppStartupSwagger()
    .UseAuthorization();

_ = app.MapControllers();

app.Run();
