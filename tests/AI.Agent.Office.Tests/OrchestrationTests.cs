using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Tests.TestSupport;
using TaskStatus = AI.Agent.Office.Domain.Enums.TaskStatus;

namespace AI.Agent.Office.Tests;

public class OrchestrationTests
{
    private const string TwoParallelTasks = """
        {"action":"delegate","message":"Voy a dividir esta solicitud.","tasks":[
          {"id":"t1","agentId":"architect","title":"Diseñar la arquitectura","task":"Diseña la arquitectura."},
          {"id":"t2","agentId":"qa-analyst","title":"Evaluar riesgos","task":"Evalúa los riesgos."}]}
        """;

    private static ChatRequestDto Ask(string text = "Necesito una arquitectura") => new(text);

    [Fact]
    public async Task Respond_action_answers_directly_without_delegating()
    {
        var h = new OfficeHarness();
        h.Script("""{"action":"respond","message":"¡Hola! ¿En qué te ayudo?","tasks":[]}""");

        var result = await h.Orchestration.HandleMessageAsync(Ask("hola"), CancellationToken.None);

        Assert.False(result.Delegated);
        Assert.False(result.IsError);
        Assert.Equal("¡Hola! ¿En qué te ayudo?", result.Reply);
        Assert.Empty(h.Tasks.GetAll());
        Assert.Single(h.Anthropic.Requests); // only the planning call
    }

    [Fact]
    public async Task Delegation_runs_independent_tasks_and_consolidates_the_answer()
    {
        var h = new OfficeHarness();
        h.Script(TwoParallelTasks);

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.True(result.Delegated);
        Assert.Equal("resultado de orchestrator:consolidate", result.Reply);
        Assert.Equal(2, result.Tasks.Count);
        Assert.All(result.Tasks, t => Assert.Equal(TaskStatus.Completed, t.Status));
        Assert.Contains(result.Tasks, t => t.AgentId == "architect" && t.Result == "resultado de agent:architect");

        var types = h.Publisher.Events.Select(e => e.EventType).ToList();
        Assert.Contains(OfficeEventType.OrchestrationStarted, types);
        Assert.Contains(OfficeEventType.TaskCreated, types);
        Assert.Equal(2, types.Count(t => t == OfficeEventType.AgentTaskAssigned));
        Assert.Equal(2, types.Count(t => t == OfficeEventType.TaskCompleted));
        Assert.Contains(OfficeEventType.OrchestrationCompleted, types);
    }

    [Fact]
    public async Task Agent_status_follows_real_events_Thinking_then_working_status_then_Completed()
    {
        var h = new OfficeHarness();
        h.Script("""
            {"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"developer","title":"Código","task":"Escribe código"}]}
            """);

        await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        var developerStates = h.Publisher.Events
            .Where(e => e.AgentId == "developer" && e.AdditionalData?.ContainsKey("agentStatus") == true)
            .Select(e => e.AdditionalData!["agentStatus"])
            .ToList();

        Assert.Equal(new[] { "Thinking", "Thinking", "Coding", "Completed" }, developerStates);
        Assert.Equal(AgentStatus.Completed, h.Registry.Get("developer")!.Status);
    }

    [Fact]
    public async Task Dependent_task_receives_the_result_of_its_prerequisite()
    {
        var h = new OfficeHarness();
        h.Script("""
            {"action":"delegate","message":"x","tasks":[
              {"id":"t1","agentId":"architect","title":"Diseño","task":"Diseña"},
              {"id":"t2","agentId":"qa-analyst","title":"Revisión","task":"Revisa el diseño","dependsOn":["t1"]}]}
            """,
            (request, _) =>
            {
                if (request.Purpose == "agent:architect") return Task.FromResult(FakeAnthropicService.Text("DISEÑO-ARQ-123"));
                return Task.FromResult(FakeAnthropicService.Text("ok:" + request.Messages[0].Content));
            });

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        var qa = result.Tasks.Single(t => t.AgentId == "qa-analyst");
        Assert.Equal(TaskStatus.Completed, qa.Status);
        Assert.Contains("DISEÑO-ARQ-123", qa.Result);
        Assert.True(result.Tasks.Single(t => t.AgentId == "architect").CompletedAt <= qa.StartedAt);
    }

    [Fact]
    public async Task A_failing_agent_does_not_break_the_others_and_blocks_only_its_dependents()
    {
        var h = new OfficeHarness();
        h.Script("""
            {"action":"delegate","message":"x","tasks":[
              {"id":"t1","agentId":"architect","title":"Diseño","task":"Diseña"},
              {"id":"t2","agentId":"developer","title":"Código","task":"Implementa"},
              {"id":"t3","agentId":"qa-analyst","title":"QA","task":"Revisa","dependsOn":["t1"]}]}
            """,
            (request, _) => request.Purpose == "agent:architect"
                ? throw new LlmException("boom")
                : Task.FromResult(FakeAnthropicService.Text("bien")));

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(TaskStatus.Failed, result.Tasks.Single(t => t.Key == "t1").Status);
        Assert.Equal("boom", result.Tasks.Single(t => t.Key == "t1").ErrorMessage);
        Assert.Equal(TaskStatus.Completed, result.Tasks.Single(t => t.Key == "t2").Status);
        Assert.Equal(TaskStatus.Cancelled, result.Tasks.Single(t => t.Key == "t3").Status);
        Assert.Contains(h.Publisher.Events, e => e.EventType == OfficeEventType.AgentError && e.AgentId == "architect");
        Assert.Equal(AgentStatus.Error, h.Registry.Get("architect")!.Status);
    }

    [Fact]
    public async Task Concurrency_is_limited_by_MaxConcurrency()
    {
        var h = new OfficeHarness(o => o.MaxConcurrency = 2);
        h.Script("""
            {"action":"delegate","message":"x","tasks":[
              {"id":"t1","agentId":"architect","title":"a","task":"a"},
              {"id":"t2","agentId":"developer","title":"b","task":"b"},
              {"id":"t3","agentId":"qa-analyst","title":"c","task":"c"},
              {"id":"t4","agentId":"pdf-reader","title":"d","task":"d"}]}
            """,
            async (request, ct) =>
            {
                if (request.Purpose == "orchestrator:consolidate") return FakeAnthropicService.Text("final");
                var now = Interlocked.Increment(ref _running);
                InterlockedMax(ref _maxRunning, now);
                await Task.Delay(120, ct);
                Interlocked.Decrement(ref _running);
                return FakeAnthropicService.Text("ok");
            });

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.All(result.Tasks, t => Assert.Equal(TaskStatus.Completed, t.Status));
        Assert.Equal(2, _maxRunning);
    }

    [Fact]
    public async Task Tasks_of_the_same_agent_never_overlap()
    {
        var h = new OfficeHarness(o => o.MaxConcurrency = 4);
        h.Script("""
            {"action":"delegate","message":"x","tasks":[
              {"id":"t1","agentId":"developer","title":"a","task":"a"},
              {"id":"t2","agentId":"developer","title":"b","task":"b"}]}
            """,
            async (request, ct) =>
            {
                if (request.Purpose == "orchestrator:consolidate") return FakeAnthropicService.Text("final");
                var now = Interlocked.Increment(ref _running);
                InterlockedMax(ref _maxRunning, now);
                await Task.Delay(80, ct);
                Interlocked.Decrement(ref _running);
                return FakeAnthropicService.Text("ok");
            });

        await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.Equal(1, _maxRunning);
    }

    [Fact]
    public async Task A_task_that_exceeds_the_timeout_fails_with_a_clear_message()
    {
        var h = new OfficeHarness(o => o.TaskTimeoutSeconds = 1);
        h.Script("""
            {"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"developer","title":"lenta","task":"x"}]}
            """,
            async (request, ct) =>
            {
                if (request.Purpose == "orchestrator:consolidate") return FakeAnthropicService.Text("final");
                await Task.Delay(Timeout.Infinite, ct);
                return FakeAnthropicService.Text("never");
            });

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        var task = Assert.Single(result.Tasks);
        Assert.Equal(TaskStatus.Failed, task.Status);
        Assert.Contains("tiempo máximo", task.ErrorMessage);
    }

    [Fact]
    public async Task A_running_task_can_be_cancelled_by_the_user()
    {
        var h = new OfficeHarness(o => o.TaskTimeoutSeconds = 30);
        h.Script("""
            {"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"developer","title":"larga","task":"x"}]}
            """,
            async (request, ct) =>
            {
                if (request.Purpose == "orchestrator:consolidate") return FakeAnthropicService.Text("final");
                await Task.Delay(Timeout.Infinite, ct);
                return FakeAnthropicService.Text("never");
            });

        var run = h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);
        await WaitUntilAsync(() => h.Tasks.GetAll().Any(t => t.Status == TaskStatus.Running));

        Assert.True(h.Orchestration.CancelTask(h.Tasks.GetAll().Single().Id));
        var result = await run;

        Assert.Equal(TaskStatus.Cancelled, Assert.Single(result.Tasks).Status);
        Assert.NotEqual(AgentStatus.Error, h.Registry.Get("developer")!.Status);
    }

    [Fact]
    public async Task Cancelling_the_whole_request_stops_planning()
    {
        var h = new OfficeHarness();
        h.Anthropic.Handler = async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return FakeAnthropicService.Text("never");
        };

        var conversation = Guid.NewGuid();
        var run = h.Orchestration.HandleMessageAsync(new ChatRequestDto("hola", conversation), CancellationToken.None);
        await WaitUntilAsync(() => h.Anthropic.Requests.Count > 0);

        Assert.True(h.Orchestration.CancelConversationRun(conversation));
        var result = await run;

        Assert.True(result.IsError);
        Assert.Contains("cancelada", result.Reply);
    }

    [Fact]
    public async Task Invalid_decision_is_corrected_with_one_retry()
    {
        var h = new OfficeHarness();
        var planningCalls = 0;
        h.Anthropic.Handler = (request, _) =>
        {
            if (request.StructuredOutput is null) return Task.FromResult(FakeAnthropicService.Text("final"));
            planningCalls++;
            return Task.FromResult(FakeAnthropicService.Decision(planningCalls == 1
                ? """{"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"no-existe","title":"x","task":"x"}]}"""
                : """{"action":"respond","message":"corregido","tasks":[]}"""));
        };

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.Equal(2, planningCalls);
        Assert.Equal("corregido", result.Reply);
    }

    [Fact]
    public async Task Persistently_invalid_decision_falls_back_to_a_safe_reply()
    {
        var h = new OfficeHarness();
        h.Script("""{"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"ghost","title":"x","task":"x"}]}""");

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.False(result.Delegated);
        Assert.Contains("reformular", result.Reply);
        Assert.Empty(h.Tasks.GetAll());
    }

    [Fact]
    public async Task Anthropic_failure_returns_a_friendly_error_and_marks_the_orchestrator()
    {
        var h = new OfficeHarness();
        h.Anthropic.Handler = (_, _) => throw new LlmException("La API key de Anthropic no es válida.", 401);

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains("API key", result.Reply);
        Assert.Equal(AgentStatus.Error, h.Registry.Get("orchestrator")!.Status);
        Assert.Contains(h.Publisher.Events, e => e.EventType == OfficeEventType.AgentError);
    }

    [Fact]
    public async Task Consolidation_failure_still_returns_the_agents_results()
    {
        var h = new OfficeHarness();
        h.Script(TwoParallelTasks, (request, _) => request.Purpose == "orchestrator:consolidate"
            ? throw new LlmException("sobrecargado")
            : Task.FromResult(FakeAnthropicService.Text("trabajo de " + request.Purpose)));

        var result = await h.Orchestration.HandleMessageAsync(Ask(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Contains("trabajo de agent:architect", result.Reply);
        Assert.Contains("trabajo de agent:qa-analyst", result.Reply);
    }

    [Fact]
    public async Task Conversation_history_is_sent_to_the_orchestrator_and_kept_in_memory()
    {
        var h = new OfficeHarness();
        h.Script("""{"action":"respond","message":"respuesta","tasks":[]}""");

        var first = await h.Orchestration.HandleMessageAsync(Ask("primero"), CancellationToken.None);
        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("segundo", first.ConversationId), CancellationToken.None);

        var lastPlanning = h.Anthropic.Requests.Last();
        Assert.Equal(new[] { "user", "assistant", "user" }, lastPlanning.Messages.Select(m => m.Role));
        Assert.Equal(4, h.Conversations.Get(first.ConversationId)!.Messages.Count);
    }

    [Fact]
    public async Task Attached_pdf_text_reaches_only_agents_that_accept_documents()
    {
        var h = new OfficeHarness();
        var doc = new AI.Agent.Office.Domain.Entities.OfficeDocument { FileName = "informe.pdf", PageCount = 2, Text = "CONTENIDO-SECRETO-DEL-PDF" };
        h.Documents.Add(doc);
        h.Script("""
            {"action":"delegate","message":"x","tasks":[
              {"id":"t1","agentId":"pdf-reader","title":"Resumir","task":"Resume el PDF"},
              {"id":"t2","agentId":"developer","title":"Otra","task":"Algo"}]}
            """);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("resume", null, new[] { doc.Id }), CancellationToken.None);

        string PromptOf(string purpose) => h.Anthropic.Requests.Single(r => r.Purpose == purpose).Messages[0].Content;
        Assert.Contains("CONTENIDO-SECRETO-DEL-PDF", PromptOf("agent:pdf-reader"));
        Assert.DoesNotContain("CONTENIDO-SECRETO-DEL-PDF", PromptOf("agent:developer"));
        // The orchestrator only learns the metadata, not the full text.
        Assert.DoesNotContain(h.Anthropic.Requests.Where(r => r.StructuredOutput is not null),
            r => r.Messages.Any(m => m.Content.Contains("CONTENIDO-SECRETO-DEL-PDF")));
    }

    [Fact]
    public async Task Empty_or_oversized_messages_are_rejected()
    {
        var h = new OfficeHarness();
        await Assert.ThrowsAsync<OfficeValidationException>(() => h.Orchestration.HandleMessageAsync(new ChatRequestDto("  "), CancellationToken.None));
        await Assert.ThrowsAsync<OfficeValidationException>(() =>
            h.Orchestration.HandleMessageAsync(new ChatRequestDto(new string('x', h.Options.MaxMessageChars + 1)), CancellationToken.None));
    }

    // ---- helpers ----
    private int _running;
    private int _maxRunning;

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref target)))
            if (Interlocked.CompareExchange(ref target, value, current) == current) break;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("La condición no se cumplió a tiempo.");
            await Task.Delay(20);
        }
    }
}
