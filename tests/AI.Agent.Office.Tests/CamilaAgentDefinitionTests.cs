using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.Camila;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Tests.TestSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Tests;

/// <summary>Spec 012 · CamilaIA como agente definido en Markdown: persona, herramientas impuestas, notas, narrativa y consumo atribuido.</summary>
public class CamilaAgentDefinitionTests
{
    private sealed class FakeAgents : IAgentConfigurationService
    {
        public List<AgentDefinition> Items { get; } = new();
        public Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new ConfigurationReport(Items, Array.Empty<ConfigurationIssue>()));
        public IReadOnlyList<AgentDefinition> GetDefinitions() => Items;
        public AgentDefinition? GetDefinition(string agentId) => Items.FirstOrDefault(a => a.Id == agentId);
        public string? GetInstructions(string agentId) => GetDefinition(agentId)?.Instructions;
        public ConfigurationReport Validate() => new(Items, Array.Empty<ConfigurationIssue>());
    }

    private sealed class Source : ICamilaBotSource
    {
        public Dictionary<string, string>? Sections { get; set; }
        public string? NotesText { get; set; }
        public IReadOnlyList<BotInfo> Bots => new[] { new BotInfo("camila", "Camila", "Bot", "🧩", "lector-uml", "camila-ia", 2, 3, 2) };
        public CamilaBot? Get(string botId) => BotPlaybookTests.Bot();
        public string? Notes(string botId) => NotesText;
        public IReadOnlyDictionary<string, string>? Narrative(string botId) => Sections;
        public IReadOnlyList<string> Problems(string botId) => Array.Empty<string>();
        public Task<int> ReloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class Transport : ICamilaHttpTransport
    {
        public int Calls;
        public Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken) { Calls++; return Task.FromResult(new CamilaHttpResponse(200, "{}")); }
    }

    private sealed class NoGen : ICamilaGeneratorRunner
    {
        public Task<string> RunAsync(CamilaGeneratorDef generator, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
    }

    private static AgentDefinition Camila(string[] tools, string kind = "llm", string env = "conversational") =>
        new("camila-ia", "CamilaIA", "Conversa", "Asistente", "camila-ia.md", "Eres CamilaDePrueba: tuteas siempre.",
            new AI.Agent.Office.Domain.ValueObjects.AgentModelSettings(null, 777, null, null, null),
            AI.Agent.Office.Domain.Enums.AgentStatus.Working, false, false, "#8b5cf6", "💬", Purpose: "Llevar la conversación y la base de conocimiento.",
            Kind: kind, Environment: env, Tools: tools, Knowledge: "camila", Sensitive: true, IsSystem: true);

    private static (CamilaWorkflow Workflow, FakeAnthropicService Llm, Transport Transport, Source Source) Build(AgentDefinition agent, Action<Source>? tweak = null)
    {
        var transport = new Transport();
        var options = Options.Create(new CamilaOptions { AllowedHosts = { "api.example.com:9876" } });
        var hooks = new CamilaWebhookService(options, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["CAMILA_WH_X"] = "x" }).Build(), transport, NullLogger<CamilaWebhookService>.Instance);
        var llm = new FakeAnthropicService();
        var source = new Source();
        tweak?.Invoke(source);
        var agents = new FakeAgents();
        agents.Items.Add(agent);
        agents.Items.Add(new AgentDefinition("lector-uml", "LectorUML", "Ejecuta", "Ejecutor", "lector-uml.md", "Ejecuta bots.", new AI.Agent.Office.Domain.ValueObjects.AgentModelSettings(),
            AI.Agent.Office.Domain.Enums.AgentStatus.Working, false, false, "#34d399", "🧩", Purpose: "Ejecutar los flujos UML de los bots.", Kind: "executor", Environment: "chatbot",
            Tools: new[] { "leer_playbook", "consultar_servicio" }, IsSystem: true));
        var workflow = new CamilaWorkflow(source, new CamilaSessionStore(), hooks, new NoGen(), llm, options, NullLogger<CamilaWorkflow>.Instance, agents);
        return (workflow, llm, transport, source);
    }

    private static void Script(FakeAnthropicService llm, params string[] decisions)
    {
        var i = 0;
        llm.Handler = (_, _) => Task.FromResult(FakeAnthropicService.Decision(decisions[Math.Min(i++, decisions.Length - 1)]));
    }

    private static readonly CamilaTarget Target = new(CamilaMode.Conversational, "camila-ia");

    [Fact]
    public async Task The_persona_notes_token_limit_privacy_and_consumption_come_from_the_agent_definition()
    {
        var (workflow, llm, _, _) = Build(Camila(new[] { "buscar_conocimiento", "consultar_servicio", "escribir_servicio" }), s => s.NotesText = "# Notas\nEl cierre de nómina es el día 25.");
        Script(llm, "{\"action\":\"responder\",\"reply\":\"Hola\"}");

        await workflow.HandleAsync(Target, Guid.NewGuid(), "hola", null, default);

        var request = Assert.Single(llm.Requests);
        Assert.Contains("Eres CamilaDePrueba: tuteas siempre.", request.System);               // la personalidad sale del .md
        Assert.Contains("Herramientas permitidas: buscar_conocimiento, consultar_servicio, escribir_servicio", request.System);
        Assert.Contains("El cierre de nómina es el día 25.", request.Messages[0].Content);      // notas.md como contexto
        Assert.Equal("agent:camila-ia", request.Purpose);                                      // el consumo se atribuye a CamilaIA
        Assert.Equal(777, request.MaxTokens);
        Assert.True(request.Sensitive);
    }

    [Fact]
    public async Task Without_the_write_tool_the_agent_cannot_write_even_if_the_model_asks_for_it()
    {
        var (workflow, llm, transport, _) = Build(Camila(new[] { "buscar_conocimiento", "consultar_servicio" }));
        Script(llm, "{\"action\":\"webhook\",\"webhook\":\"Guardar nota\"}", "{\"action\":\"responder\",\"reply\":\"No puedo guardar eso.\"}");

        var reply = await workflow.HandleAsync(Target, Guid.NewGuid(), "guarda mi nota", null, default);

        Assert.Equal("No puedo guardar eso.", reply.Reply);
        Assert.Equal(0, transport.Calls);
        Assert.DoesNotContain("necesito tu confirmación", reply.Reply);                        // ni siquiera se llega a pedir confirmación
        Assert.Contains("[No permitido] No tienes la herramienta escribir_servicio", llm.Requests[1].Messages[0].Content);
    }

    [Fact]
    public async Task Without_the_search_tool_the_agent_cannot_search_the_knowledge_base()
    {
        var (workflow, llm, _, _) = Build(Camila(new[] { "consultar_servicio" }));
        Script(llm, "{\"action\":\"buscar\",\"query\":\"convenios\"}", "{\"action\":\"responder\",\"reply\":\"No tengo esa información.\"}");

        await workflow.HandleAsync(Target, Guid.NewGuid(), "¿hay convenios?", null, default);

        Assert.Contains("[No permitido] No tienes la herramienta buscar_conocimiento", llm.Requests[1].Messages[0].Content);
        Assert.DoesNotContain("[Resultado de buscar en la base]", llm.Requests[1].Messages[0].Content);
    }

    [Fact]
    public async Task Searching_returns_the_narrative_section_of_the_knowledge_base_when_there_is_one()
    {
        var (workflow, llm, _, _) = Build(Camila(new[] { "buscar_conocimiento" }),
            s => s.Sections = new Dictionary<string, string> { ["Convenios y beneficios"] = "NARRATIVA-DE-CONVENIOS: alianzas con comercios." });
        Script(llm, "{\"action\":\"buscar\",\"query\":\"convenios beneficios\"}", "{\"action\":\"responder\",\"reply\":\"Hay alianzas.\"}");

        await workflow.HandleAsync(Target, Guid.NewGuid(), "¿qué convenios hay?", null, default);

        Assert.Contains("NARRATIVA-DE-CONVENIOS: alianzas con comercios.", llm.Requests[1].Messages[0].Content);
    }

    [Theory]
    [InlineData("executor", "conversational")]
    [InlineData("llm", "office")]
    public async Task Only_conversational_agents_with_a_model_open_a_conversation(string kind, string environment)
    {
        var (workflow, llm, _, _) = Build(Camila(new[] { "buscar_conocimiento" }, kind, environment));

        var reply = await workflow.HandleAsync(Target, Guid.NewGuid(), "hola", null, default);

        Assert.True(reply.IsError);
        Assert.Contains("no es un agente conversacional", reply.Reply);
        Assert.Empty(llm.Requests);
    }

    [Fact]
    public async Task A_bot_is_executed_by_its_executor_agent_and_the_target_is_validated_before_starting()
    {
        var (workflow, _, _, _) = Build(Camila(new[] { "buscar_conocimiento" }));

        Assert.Equal("lector-uml", workflow.ExecutorOf(new CamilaTarget(CamilaMode.Uml, "camila")));
        Assert.Equal("camila-ia", workflow.ExecutorOf(Target));
        Assert.Null(await workflow.ValidateAsync(new CamilaTarget(CamilaMode.Uml, "camila"), default));
        Assert.Contains("No encuentro el playbook del bot «nada»", await workflow.ValidateAsync(new CamilaTarget(CamilaMode.Uml, "nada"), default) ?? string.Empty);
        Assert.Contains("no existe", await workflow.ValidateAsync(new CamilaTarget(CamilaMode.Conversational, "fantasma"), default) ?? string.Empty);
    }
}
