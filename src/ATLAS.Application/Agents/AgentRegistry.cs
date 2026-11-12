using ATLAS.Application.Interfaces;
using ATLAS.Domain.Enums;

namespace ATLAS.Application.Agents;

using Agent = ATLAS.Domain.Entities.Agent;

/// <summary>Registro en memoria (seguro entre hilos) de los agentes lógicos y su estado en tiempo de ejecución.</summary>
public sealed class AgentRegistry : IAgentRegistry
{
    private readonly object _gate = new();
    private readonly List<Agent> _agents = new();

    /// <summary>Carga o recarga los agentes desde sus definiciones.</summary>
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
                    ProducesWord = d.ProducesWord,
                    WorkflowOnly = d.WorkflowOnly,
                    Color = d.Color,
                    Icon = d.Icon,
                    // El estado en ejecución se conserva al recargar la configuración.
                    Status = existing?.Status ?? AgentStatus.Idle,
                    CurrentTaskId = existing?.CurrentTaskId,
                    CurrentTaskTitle = existing?.CurrentTaskTitle,
                    LastActivityAt = existing?.LastActivityAt
                });
            }
        }
    }

    /// <summary>Todos los agentes registrados.</summary>
    public IReadOnlyList<Agent> GetAll()
    {
        lock (_gate) { return _agents.Select(a => a.Clone()).ToArray(); }
    }

    /// <summary>Busca un agente por su id.</summary>
    public Agent? Get(string agentId)
    {
        lock (_gate) { return Find(agentId)?.Clone(); }
    }

    /// <summary>Aplica un cambio al agente de forma atómica y lo devuelve.</summary>
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
