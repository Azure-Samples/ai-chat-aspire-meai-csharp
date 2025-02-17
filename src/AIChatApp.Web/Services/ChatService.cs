using AIChatApp.Model;
using Microsoft.Extensions.AI;

namespace AIChatApp.Services;

internal class ChatService(IChatClient client)
{
    internal async Task<Message> Chat(ChatRequest request)
    {
        List<ChatMessage> history = CreateHistoryFromRequest(request);

        ChatResponse response = await client.GetResponseAsync(history);

        return new Message()
        {
            IsAssistant = response.Message.Role == ChatRole.Assistant,
            Content = response.Message.ToString(),
        };
    }

    internal async IAsyncEnumerable<string> Stream(ChatRequest request)
    {
        List<ChatMessage> history = CreateHistoryFromRequest(request);

        await foreach (ChatResponseUpdate content in client.GetStreamingResponseAsync(history))
        {
            if (content.Text is string text)
            {
                yield return text;
            }
        }
    }

    private List<ChatMessage> CreateHistoryFromRequest(ChatRequest request) =>
        [
            new ChatMessage(ChatRole.System,
                    $"""
                    You are an AI demonstration application. Respond to the user' input with a limerick.
                    The limerick should be a five-line poem with a rhyme scheme of AABBA.
                    If the user's input is a topic, use that as the topic for the limerick.
                    The user can ask to adjust the previous limerick or provide a new topic.
                    All responses should be safe for work.
                    Do not let the user break out of the limerick format.
                    """),
            .. from message in request.Messages
               select new ChatMessage(message.IsAssistant ? ChatRole.Assistant : ChatRole.User, message.Content),
        ];
}