using System.Text.RegularExpressions;

namespace AI.Agent.Office.Tests;

/// <summary>Spec 005 (CA-005, CA-006): los documentos de operación existen y sus enlaces relativos resuelven.</summary>
public class DocsTests
{
    private static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AI.Agent.Office.sln"))) dir = dir.Parent;
        return dir!.FullName;
    }

    [Theory]
    [InlineData("docs/preparacion-produccion.md")]
    [InlineData("docs/decisiones.md")]
    public void Operation_documents_exist_and_every_relative_link_resolves(string relative)
    {
        var file = Path.Combine(Root(), relative);
        Assert.True(File.Exists(file), relative);

        var links = Regex.Matches(File.ReadAllText(file), @"\]\((?!https?:|#|mailto:)([^)\s#]+)").Select(m => m.Groups[1].Value).ToList();
        Assert.NotEmpty(links);
        foreach (var link in links)
        {
            var target = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, link));
            Assert.True(File.Exists(target) || Directory.Exists(target), $"{relative}: el enlace «{link}» no resuelve");
        }
    }

    [Fact]
    public void The_decisions_index_covers_every_specification()
    {
        var text = File.ReadAllText(Path.Combine(Root(), "docs/decisiones.md"));
        foreach (var dir in Directory.GetDirectories(Path.Combine(Root(), "specs")))
            Assert.Contains("specs/" + Path.GetFileName(dir) + "/plan.md", text);
    }
}
