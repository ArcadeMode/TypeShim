var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Host_Server>("host");

builder.Build().Run();
