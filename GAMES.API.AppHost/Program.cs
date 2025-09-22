var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.GAMES_API>("games-api");

builder.Build().Run();
