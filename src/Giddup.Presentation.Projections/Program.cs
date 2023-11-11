// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Presentation.Projections;

var app = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddHostedService<PullRequestProjectionWorker>())
    .Build();

app.Run();
