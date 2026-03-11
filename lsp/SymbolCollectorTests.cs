using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using UnityCli.Lsp.Core;
using Xunit;

public sealed class SymbolCollectorTests
{
    [Fact]
    public void CollectEntries_FindsMajorSymbolKinds()
    {
        const string code = """
using System;
namespace Sample;
public class Player
{
    public int Health;
    public Player() {}
    public int Value { get; set; }
    public void Jump() {}
}
""";
        var tree = CSharpSyntaxTree.ParseText(code);
        var collector = new LspSymbolCollector();
        var entries = collector.CollectEntries(tree.GetRoot(), "Assets/Scripts/Player.cs");

        Assert.Contains(entries, entry => entry.Kind == "class" && entry.Name == "Player");
        Assert.Contains(entries, entry => entry.Kind == "field" && entry.Name == "Health");
        Assert.Contains(entries, entry => entry.Kind == "constructor" && entry.Name == "Player");
        Assert.Contains(entries, entry => entry.Kind == "property" && entry.Name == "Value");
        Assert.Contains(entries, entry => entry.Kind == "method" && entry.Name == "Jump");
    }

    [Fact]
    public void MakeSym_UsesContainerMetadata()
    {
        var entry = new CodeIndexEntry
        {
            Name = "Jump",
            Kind = "method",
            NamePath = "Player/Jump",
            File = "Assets/Scripts/Player.cs",
            Line = 5,
            Column = 7
        };

        var symbol = LspCodeIndexModels.MakeSym(entry);
        var container = symbol.GetType().GetProperty("container")!.GetValue(symbol)?.ToString();
        var namePath = symbol.GetType().GetProperty("namePath")!.GetValue(symbol)?.ToString();

        Assert.Equal("Player", container);
        Assert.Equal("Player/Jump", namePath);
    }

    [Theory]
    [InlineData("namespace", 3)]
    [InlineData("class", 5)]
    [InlineData("method", 6)]
    [InlineData("constructor", 9)]
    [InlineData("property", 7)]
    [InlineData("field", 8)]
    [InlineData("enum", 10)]
    [InlineData("interface", 11)]
    [InlineData("struct", 23)]
    [InlineData("unknown", 0)]
    public void SymbolKindCode_MapsExpectedValues(string kind, int expected)
    {
        Assert.Equal(expected, LspCodeIndexModels.SymbolKindCode(kind));
    }

    [Fact]
    public void AddEntry_WritesNormalizedRelativePath()
    {
        var entries = new List<CodeIndexEntry>();
        var tree = CSharpSyntaxTree.ParseText("class Player {}");
        var root = tree.GetRoot();
        var cls = root.DescendantNodes().Single();

        LspCodeIndexModels.AddEntry("Player", "class", cls, new Stack<string>(), entries, @"Assets\Scripts\Player.cs");

        Assert.Equal("Assets/Scripts/Player.cs", entries.Single().File);
    }

    [Fact]
    public void AddEntry_IgnoresBlankNames()
    {
        var entries = new List<CodeIndexEntry>();
        var tree = CSharpSyntaxTree.ParseText("class Player {}");
        var root = tree.GetRoot();
        var cls = root.DescendantNodes().Single();

        LspCodeIndexModels.AddEntry(" ", "class", cls, new Stack<string>(), entries, "Assets/Scripts/Player.cs");

        Assert.Empty(entries);
    }

    [Fact]
    public void CollectEntries_CoversStructInterfaceEnumEventAndDelegate()
    {
        const string code = """
namespace Sample
{
    public interface IPlayer { int Score { get; } }
    public struct PlayerConfig { public int Speed; }
    public enum State { Idle, Run }
    public delegate void PlayerAction();
    public class Signals
    {
        public event System.Action? Changed;
        public event System.Action? Triggered, Reset;
    }
}
""";

        var tree = CSharpSyntaxTree.ParseText(code);
        var collector = new LspSymbolCollector();
        var entries = collector.CollectEntries(tree.GetRoot(), "Assets/Scripts/Signals.cs");

        Assert.Contains(entries, entry => entry.Kind == "interface" && entry.Name == "IPlayer");
        Assert.Contains(entries, entry => entry.Kind == "struct" && entry.Name == "PlayerConfig");
        Assert.Contains(entries, entry => entry.Kind == "field" && entry.Name == "Speed");
        Assert.Contains(entries, entry => entry.Kind == "enum" && entry.Name == "State");
        Assert.Contains(entries, entry => entry.Kind == "enumMember" && entry.Name == "Idle");
        Assert.Contains(entries, entry => entry.Kind == "enumMember" && entry.Name == "Run");
        Assert.Contains(entries, entry => entry.Kind == "delegate" && entry.Name == "PlayerAction");
        Assert.Contains(entries, entry => entry.Kind == "event" && entry.Name == "Changed");
        Assert.Contains(entries, entry => entry.Kind == "event" && entry.Name == "Triggered");
        Assert.Contains(entries, entry => entry.Kind == "event" && entry.Name == "Reset");
    }
}
