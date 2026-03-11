using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UnityCli.Lsp.Core;

public static class LspEditResultFactory
{
    public static string DiffPreview(string oldText, string newText)
    {
        if (newText.Length > 1000)
        {
            return newText[..1000] + "…";
        }

        return newText;
    }

    public static string MaybeFormatText(string text, bool format)
    {
        if (!format)
        {
            return text;
        }

        var tree = CSharpSyntaxTree.ParseText(text);
        return tree.GetRoot().NormalizeWhitespace().ToFullString();
    }

    public static List<Dictionary<string, object?>> CollectSyntaxDiagnostics(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text ?? string.Empty);
        var diagnostics = new List<Dictionary<string, object?>>();
        foreach (var diag in tree.GetDiagnostics())
        {
            var span = diag.Location.GetLineSpan();
            diagnostics.Add(new Dictionary<string, object?>
            {
                ["severity"] = diag.Severity.ToString().ToLowerInvariant(),
                ["id"] = diag.Id,
                ["message"] = diag.GetMessage(),
                ["line"] = span.StartLinePosition.Line + 1,
                ["column"] = span.StartLinePosition.Character + 1
            });
        }

        return diagnostics;
    }

    public static bool HasErrorDiagnostics(IEnumerable<Dictionary<string, object?>> diagnostics) =>
        diagnostics.Any(diag =>
        {
            diag.TryGetValue("severity", out var severity);
            return string.Equals(severity?.ToString(), "error", StringComparison.OrdinalIgnoreCase);
        });

    public static object[] BuildDiffPreviewEntries(IEnumerable<(string path, string originalText, string newText)> changes) =>
        changes
            .Select(change => (object)new Dictionary<string, object?>
            {
                ["path"] = LspPathUtilities.NormalizeRelative(change.path),
                ["preview"] = DiffPreview(change.originalText, change.newText)
            })
            .ToArray();

    public static Dictionary<string, object?> EditResult(
        bool success,
        bool applied,
        IEnumerable<string>? changedFiles = null,
        IEnumerable<string>? changedSymbols = null,
        IEnumerable<object>? diagnostics = null,
        IEnumerable<object>? diffPreview = null,
        string? reason = null)
    {
        return new Dictionary<string, object?>
        {
            ["success"] = success,
            ["applied"] = applied,
            ["changedFiles"] = (changedFiles ?? Array.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            ["changedSymbols"] = (changedSymbols ?? Array.Empty<string>())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            ["diagnostics"] = (diagnostics ?? Array.Empty<object>()).ToArray(),
            ["diffPreview"] = (diffPreview ?? Array.Empty<object>()).ToArray(),
            ["reason"] = reason
        };
    }
}
