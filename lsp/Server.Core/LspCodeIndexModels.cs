using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace UnityCli.Lsp.Core;

[ExcludeFromCodeCoverage]
public sealed class CodeIndexDocument
{
    public string GeneratedAt { get; set; } = string.Empty;
    public string Root { get; set; } = string.Empty;
    public CodeIndexEntry[] Entries { get; set; } = Array.Empty<CodeIndexEntry>();
}

[ExcludeFromCodeCoverage]
public sealed class CodeIndexEntry
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string NamePath { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public string? Summary { get; set; }
}

public static class LspCodeIndexModels
{
    public static int SymbolKindCode(string kind) => kind switch
    {
        "namespace" => 3,
        "class" => 5,
        "method" => 6,
        "constructor" => 9,
        "property" => 7,
        "field" => 8,
        "enum" => 10,
        "interface" => 11,
        "struct" => 23,
        _ => 0
    };

    public static object MakeSym(CodeIndexEntry entry)
    {
        var start = new { line = Math.Max(entry.Line - 1, 0), character = Math.Max(entry.Column - 1, 0) };
        var end = start;
        return new
        {
            name = entry.Name,
            kind = SymbolKindCode(entry.Kind),
            kindName = entry.Kind,
            namePath = LspPathUtilities.NormalizeNamePath(entry.NamePath),
            container = LspPathUtilities.ContainerName(entry.NamePath),
            containerPath = LspPathUtilities.ContainerPath(entry.NamePath),
            range = new { start, end },
            selectionRange = new { start, end }
        };
    }

    public static void AddEntry(string name, string kind, SyntaxNode node, Stack<string> scope, List<CodeIndexEntry> output, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var span = node.GetLocation().GetLineSpan();
        output.Add(new CodeIndexEntry
        {
            Name = name,
            Kind = kind,
            NamePath = LspPathUtilities.NormalizeNamePath(string.Join('/', scope.Reverse().Append(name))),
            File = LspPathUtilities.NormalizeRelative(relativePath),
            Line = span.StartLinePosition.Line + 1,
            Column = span.StartLinePosition.Character + 1
        });
    }
}
