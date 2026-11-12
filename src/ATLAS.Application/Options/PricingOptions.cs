namespace ATLAS.Application.Options;

/// <summary>
/// Precio opcional en USD por millón de tokens, usado solo para mostrar un costo ESTIMADO. Déjalo en 0
/// para ocultarlo. Toma los valores de la página oficial de precios de Anthropic para el modelo que
/// uses.
/// </summary>
public sealed class PricingOptions
{
    public const string SectionName = "Pricing";

    /// <summary>USD por millón de tokens de entrada.</summary>
    public decimal InputPerMillionTokens { get; set; }
    /// <summary>USD por millón de tokens de salida.</summary>
    public decimal OutputPerMillionTokens { get; set; }
}
