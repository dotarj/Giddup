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

        options.SupportNonNullableReferenceTypes();

        options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Giddup API" });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Giddup API v1"));
}

app.MapControllers();

app.Run();
