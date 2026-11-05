using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Application.Agents;

using Agent = AI.Agent.Office.Domain.Entities.Agent;

/// <summary>Thread-safe, in-memory registry of the logical agents.</summary>
public sealed class AgentRegistry : IAgentRegistry
{
    private readonly object _gate = new();
    private readonly List<Agent> _agents = new();

    public void Load(IEnumerable<AgentDefinition> definitions)
    {
        lock (_gate)
        {
            var previous = _agents.ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);
            _agents.Clear();

            foreach (var d in definitions)
            {
                previous.TryGetValue(d.Id, out var existing);
                _agents.Add(new Agent
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Role = d.Role,
                    ConfigurationFile = d.ConfigurationFile,
                    ModelSettings = d.ModelSettings,
                    WorkingStatus = d.WorkingStatus,
                    IsOrchestrator = d.IsOrchestrator,
                    AcceptsDocuments = d.AcceptsDocuments,
                    Color = d.Color,
                    Icon = d.Icon,
                    // Runtime state survives a configuration reload.
                    Status = existing?.Status ?? AgentStatus.Idle,
                    CurrentTaskId = existing?.CurrentTaskId,
                    CurrentTaskTitle = existing?.CurrentTaskTitle,
                    LastActivityAt = existing?.LastActivityAt
                });
            }
        }
    }

    public IReadOnlyList<Agent> GetAll()
    {
        lock (_gate) { return _agents.Select(a => a.Clone()).ToArray(); }
    }

    public Agent? Get(string agentId)
    {
        lock (_gate) { return Find(agentId)?.Clone(); }
    }

    public Agent? Update(string agentId, Action<Agent> mutation)
    {
        lock (_gate)
        {
            var agent = Find(agentId);
            if (agent is null) return null;
            mutation(agent);
            return agent.Clone();
        }
    }

    private Agent? Find(string agentId) =>
        _agents.FirstOrDefault(a => string.Equals(a.Id, agentId, StringComparison.OrdinalIgnoreCase));
}
