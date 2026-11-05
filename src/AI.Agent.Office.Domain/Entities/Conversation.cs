namespace AI.Agent.Office.Domain.Entities;

public sealed class Conversation
{
    private readonly List<ChatMessage> _messages = new();
    private readonly object _gate = new();

    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<ChatMessage> Messages
    {
        get { lock (_gate) { return _messages.ToArray(); } }
    }

    public void Add(ChatMessage message)
    {
        lock (_gate) { _messages.Add(message); }
    }
}
