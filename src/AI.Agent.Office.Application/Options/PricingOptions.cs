namespace AI.Agent.Office.Application.Options;

/// <summary>
/// Optional USD price per million tokens, used only to show an ESTIMATED cost. Leave at 0 to hide it.
/// Take the values from the official Anthropic pricing page for the model you use.
/// </summary>
public sealed class PricingOptions
{
    public const string SectionName = "Pricing";

    public decimal InputPerMillionTokens { get; set; }
    public decimal OutputPerMillionTokens { get; set; }
}
