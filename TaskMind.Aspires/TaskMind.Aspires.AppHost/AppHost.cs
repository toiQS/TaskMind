var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TaskMind_APIs>("taskmind-apis");

builder.Build().Run();
