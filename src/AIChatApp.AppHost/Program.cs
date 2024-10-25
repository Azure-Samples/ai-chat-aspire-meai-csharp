using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Projects;
using System.Collections.Immutable;
using System.Security.Cryptography;

var builder = DistributedApplication.CreateBuilder(args)
    .WithCodespacesSupport(); 

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

// WORKAROUND: Enables GitHub Codespaces when running in that environment. This will be fixed in a future .NET Aspire release.
public static class CodespaceExtensions
{
    public static IDistributedApplicationBuilder WithCodespacesSupport(this IDistributedApplicationBuilder builder)
    {
        if (!builder.Configuration.GetValue<bool>("CODESPACES"))
        {
            return builder;
        }

        builder.Eventing.Subscribe<BeforeStartEvent>((e, ct) => {
            _ = Task.Run(() => UrlRewriterAsync(e.Services, ct));
            return Task.CompletedTask;
        });

        return builder;
    }

    private static async Task UrlRewriterAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var gitHubCodespacesPortForwardingDomain = configuration.GetValue<string>("GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN") ?? throw new DistributedApplicationException("Codespaces was detected but GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN environment missing.");
        var codespaceName = configuration.GetValue<string>("CODESPACE_NAME") ?? throw new DistributedApplicationException("Codespaces was detected but CODESPACE_NAME environment missing.");

        var rns = services.GetRequiredService<ResourceNotificationService>();

        var resourceEvents = rns.WatchAsync(cancellationToken);

        await foreach (var resourceEvent in resourceEvents)
        {
            Dictionary<UrlSnapshot, UrlSnapshot>? remappedUrls = null;

            foreach (var originalUrlSnapshot in resourceEvent.Snapshot.Urls)
            {
                var uri = new Uri(originalUrlSnapshot.Url);

                if (!originalUrlSnapshot.IsInternal && (uri.Scheme == "http" || uri.Scheme == "https") && uri.Host == "localhost")
                {
                    if (remappedUrls is null)
                    {
                        remappedUrls = new();
                    }

                    var newUrlSnapshot = originalUrlSnapshot with {
                        Url = $"{uri.Scheme}://{codespaceName}-{uri.Port}.{gitHubCodespacesPortForwardingDomain}{uri.AbsolutePath}"
                    };

                    remappedUrls.Add(originalUrlSnapshot, newUrlSnapshot);
                }
            }

            if (remappedUrls is not null)
            {
                var transformedUrls = from originalUrl in resourceEvent.Snapshot.Urls
                                      select remappedUrls.TryGetValue(originalUrl, out var remappedUrl) ? remappedUrl : originalUrl;

                await rns.PublishUpdateAsync(resourceEvent.Resource, resourceEvent.ResourceId, s => s with {
                    Urls = transformedUrls.ToImmutableArray()
                });
            }
        }
    }
}