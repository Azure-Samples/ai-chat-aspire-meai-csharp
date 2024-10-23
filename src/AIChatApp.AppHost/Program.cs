using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var chatDeploymentName = "chat";

// The connection string may be set in configuration to use a pre-existing deployment;
// it's also set automatically when provisioning this sample using the included bicep code
// (see the README for details)
var connectionString = builder.Configuration.GetConnectionString("openai");

var openai = String.IsNullOrEmpty(connectionString) || builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureOpenAI("openai")
         .AddDeployment(new AzureOpenAIDeployment(chatDeploymentName, "gpt-4o", "2024-05-13", "Standard", 10))
    : builder.AddConnectionString("openai");

builder.AddProject<AIChatApp_Web>("aichatapp-web")
    .WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithExternalHttpEndpoints();

builder.Build().Run();
