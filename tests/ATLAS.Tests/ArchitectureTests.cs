using System.Xml.Linq;

namespace ATLAS.Tests;

/// <summary>
/// Verifica automáticamente las reglas de dependencia entre capas (constitución Art. 6–7; spec 005, RF-008, RNF-002):
/// las dependencias apuntan al núcleo y la capa de presentación (Web) nunca ve Infrastructure.
/// </summary>
public class ArchitectureTests
{
    private const string Prefix = "ATLAS.";

    /// <summary>Referencias de proyecto permitidas por capa (lo que cada proyecto puede referenciar directamente).</summary>
    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        ["Domain"] = Array.Empty<string>(),
        ["Application"] = new[] { "Domain" },
        ["Infrastructure"] = new[] { "Application", "Domain" },
        ["Api"] = new[] { "Application", "Domain", "Infrastructure" },       // raíz de composición
        ["Web"] = new[] { "Api", "Application", "Domain" }                    // presentación: sin Infrastructure
    };

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src"))) dir = dir.Parent;
        return dir!.FullName;
    }

    private static IEnumerable<string> References(string layer)
    {
        var csproj = Path.Combine(RepoRoot(), "src", Prefix + layer, Prefix + layer + ".csproj");
        return XDocument.Load(csproj).Descendants("ProjectReference")
            .Select(r => ((string)r.Attribute("Include")!).Replace('\\', '/').Split('/').Last())
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .Select(n => n.Replace(Prefix, string.Empty))
            .ToList();
    }

    [Theory]
    [InlineData("Domain")]
    [InlineData("Application")]
    [InlineData("Infrastructure")]
    [InlineData("Api")]
    [InlineData("Web")]
    public void Each_project_only_references_what_its_layer_allows(string layer)
    {
        var unexpected = References(layer).Where(r => !Allowed[layer].Contains(r)).ToList();

        Assert.True(unexpected.Count == 0, $"{layer} referencia proyectos no permitidos: {string.Join(", ", unexpected)}");
    }

    [Fact]
    public void Domain_and_Application_never_use_outer_layers_in_code()
    {
        foreach (var (layer, forbidden) in new[] { ("Domain", new[] { "Application", "Infrastructure", "Api", "Web" }), ("Application", new[] { "Infrastructure", "Api", "Web" }) })
        {
            var offenders = SourceFiles(layer, "*.cs")
                .Where(f => forbidden.Any(x => File.ReadAllText(f).Contains(Prefix + x)))
                .Select(Path.GetFileName).ToList();

            Assert.True(offenders.Count == 0, $"{layer} usa capas externas en: {string.Join(", ", offenders)}");
        }
    }

    [Fact]
    public void Web_never_mentions_Infrastructure_in_code_or_components()
    {
        var offenders = SourceFiles("Web", "*.cs").Concat(SourceFiles("Web", "*.razor"))
            .Where(f => File.ReadAllText(f).Contains(Prefix + "Infrastructure"))
            .Select(Path.GetFileName).ToList();

        Assert.True(offenders.Count == 0, $"Web menciona Infrastructure en: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Web_does_not_declare_its_own_markdown_dependency_the_renderer_comes_from_the_application_contract()
    {
        var csproj = File.ReadAllText(Path.Combine(RepoRoot(), "src", Prefix + "Web", Prefix + "Web.csproj"));

        Assert.DoesNotContain("Markdig", csproj);
    }

    private static IEnumerable<string> SourceFiles(string layer, string pattern) =>
        Directory.EnumerateFiles(Path.Combine(RepoRoot(), "src", Prefix + layer), pattern, SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
}
