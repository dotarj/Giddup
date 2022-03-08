// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Application;
using Giddup.Application.PullRequests;
using Giddup.Domain.PullRequests;
using Giddup.Infrastructure;
using Giddup.Infrastructure.JsonConverters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IPullRequestService, PullRequestService>()
    .AddSingleton<IEventStream, EventStream>();

builder.Services
    .AddOptions<EventStoreClientOptions>()
    .Bind(builder.Configuration.GetSection(nameof(EventStoreClientOptions)))
    .ValidateDataAnnotations();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new BranchNameJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TitleJsonConverter());
    });

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.MapType(typeof(BranchName), () => new OpenApiSchema { Type = "string" });
        options.MapType(typeof(Title), () => new OpenApiSchema { Type = "string" });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
