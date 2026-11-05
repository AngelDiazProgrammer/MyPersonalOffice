using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Tests.TestSupport;

namespace AI.Agent.Office.Tests;

public class TokenUsageTests
{
    [Fact]
    public void Usage_is_grouped_per_agent_and_orchestrator_phases_are_merged()
    {
        var tracker = new TokenUsageTracker();
        tracker.Record("orchestrator:plan", 100, 20);
        tracker.Record("orchestrator:consolidate", 300, 80);
        tracker.Record("agent:developer", 500, 200);
        tracker.Record("agent:developer", 10, 5);

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(910, snapshot.InputTokens);
        Assert.Equal(305, snapshot.OutputTokens);
        Assert.Equal(4, snapshot.Requests);
        Assert.Equal(new AgentUsage(400, 100, 2), snapshot.ByAgent["orchestrator"]);
        Assert.Equal(new AgentUsage(510, 205, 2), snapshot.ByAgent["developer"]);
    }

    [Fact]
    public async Task Tracker_is_thread_safe()
    {
        var tracker = new TokenUsageTracker();
        await Task.WhenAll(Enumerable.Range(0, 500).Select(_ => Task.Run(() => tracker.Record("agent:qa-analyst", 2, 1))));

        var snapshot = tracker.GetSnapshot();
        Assert.Equal(1000, snapshot.InputTokens);
        Assert.Equal(500, snapshot.ByAgent["qa-analyst"].Requests);
    }

    [Fact]
    public void Estimated_cost_is_hidden_without_pricing_and_computed_with_it()
    {
        var h = new OfficeHarness();
        var tracker = new TokenUsageTracker();
        tracker.Record("agent:developer", 2_000_000, 1_000_000);

        OfficeQueryService Create(PricingOptions p) => new(h.Registry, h.Tasks, h.Activity, h.Conversations, tracker,
            h.Anthropic, Microsoft.Extensions.Options.Options.Create(p));

        Assert.Null(Create(new PricingOptions()).GetUsage().EstimatedCostUsd);
        var usage = Create(new PricingOptions { InputPerMillionTokens = 3m, OutputPerMillionTokens = 15m }).GetUsage();
        Assert.Equal(21m, usage.EstimatedCostUsd);
        Assert.Equal("developer", Assert.Single(usage.ByAgent).AgentId);
    }
}
