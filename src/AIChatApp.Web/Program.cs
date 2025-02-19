using AIChatApp.Components;
using AIChatApp.Model;
using AIChatApp.Services;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add the Azure OpenAI client configured in AppHost
builder.AddAzureOpenAIClient("openai");

// Add Microsoft.Extensions.AI. This will use the OpenAI client configured in the line above
var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "chat";

builder.Services.AddChatClient(services => services
    .GetRequiredService<AzureOpenAIClient>()
    .AsChatClient(chatDeploymentName)
    .AsBuilder()
    .UseOpenTelemetry());

builder.Services.AddTransient<ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Configure APIs for chat related features
// Uncomment for a non-streaming response
//app.MapPost("/api/chat", (ChatRequest request, ChatHandler chatHandler) => (chatHandler.); 
//  .WithName("Chat")
//  .WithOpenApi();
app.MapPost("/api/chat/stream", (ChatRequest request, ChatService chatHandler) => chatHandler.Stream(request))
    .WithName("StreamingChat")
    .WithOpenApi();

app.Run();