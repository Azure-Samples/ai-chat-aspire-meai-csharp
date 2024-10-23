using System;

namespace AIChatApp.Model;

public record ChatRequest(List<Message> Messages);
public class Message
{
    public required bool IsAssistant { get; set; }
    public required string Content { get; set; }
}
