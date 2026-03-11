using System.Collections.Generic;
using System.Linq;
using UnityCli.Lsp.Core;
using Xunit;

public sealed class EditResultFactoryTests
{
    [Fact]
    public void DiffPreview_TruncatesLongText()
    {
        var preview = LspEditResultFactory.DiffPreview(string.Empty, new string('a', 1205));
        Assert.Equal(1001, preview.Length);
        Assert.EndsWith("…", preview);
    }

    [Fact]
    public void MaybeFormatText_FormatsCSharp()
    {
        var formatted = LspEditResultFactory.MaybeFormatText("class C{void M(){}}", format: true);
        Assert.Contains("class C", formatted);
        Assert.Contains("void M()", formatted);
    }

    [Fact]
    public void MaybeFormatText_ReturnsOriginalWhenFormattingDisabled()
    {
        const string code = "class C{void M(){}}";
        Assert.Equal(code, LspEditResultFactory.MaybeFormatText(code, format: false));
    }

    [Fact]
    public void CollectSyntaxDiagnostics_FindsParseErrors()
    {
        var diagnostics = LspEditResultFactory.CollectSyntaxDiagnostics("class C {");
        Assert.NotEmpty(diagnostics);
        Assert.True(LspEditResultFactory.HasErrorDiagnostics(diagnostics));
    }

    [Fact]
    public void HasErrorDiagnostics_ReturnsFalseForWarningsOnlyPayload()
    {
        var diagnostics = new[]
        {
            new Dictionary<string, object?> { ["severity"] = "warning" }
        };
        Assert.False(LspEditResultFactory.HasErrorDiagnostics(diagnostics));
    }

    [Fact]
    public void BuildDiffPreviewEntries_NormalizesPaths()
    {
        var entries = LspEditResultFactory.BuildDiffPreviewEntries(new[] { (@"Assets\A.cs", "old", "new") });
        var item = Assert.IsType<Dictionary<string, object?>>(entries.Single());
        Assert.Equal("Assets/A.cs", item["path"]);
        Assert.Equal("new", item["preview"]);
    }

    [Fact]
    public void EditResult_DeduplicatesFilesAndSymbols()
    {
        var result = LspEditResultFactory.EditResult(
            success: true,
            applied: false,
            changedFiles: new[] { "Assets/A.cs", "Assets/A.cs" },
            changedSymbols: new[] { "Player/Jump", "Player/Jump" },
            diagnostics: new object[] { "warn" },
            reason: "preview_only");

        Assert.True((bool)result["success"]!);
        Assert.False((bool)result["applied"]!);
        Assert.Single((string[])result["changedFiles"]!);
        Assert.Single((string[])result["changedSymbols"]!);
        Assert.Equal("preview_only", result["reason"]);
    }
}
