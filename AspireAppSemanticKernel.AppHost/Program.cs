var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebApiSemanticKernel>("webapisemantickernel");

builder.Build().Run();
