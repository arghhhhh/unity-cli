using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityCli.Lsp.Core;

public sealed class LspSymbolCollector
{
    public List<CodeIndexEntry> CollectEntries(SyntaxNode root, string relativePath)
    {
        var entries = new List<CodeIndexEntry>();
        CollectSymbols(root, new Stack<string>(), entries, relativePath);
        return entries;
    }

    public void CollectSymbols(SyntaxNode node, Stack<string> scope, List<CodeIndexEntry> output, string relativePath)
    {
        switch (node)
        {
            case CompilationUnitSyntax compilationUnit:
                foreach (var member in compilationUnit.Members)
                {
                    CollectSymbols(member, scope, output, relativePath);
                }
                break;
            case FileScopedNamespaceDeclarationSyntax fileNs:
                foreach (var member in fileNs.Members)
                {
                    CollectSymbols(member, scope, output, relativePath);
                }
                break;
            case NamespaceDeclarationSyntax ns:
                foreach (var member in ns.Members)
                {
                    CollectSymbols(member, scope, output, relativePath);
                }
                break;
            case ClassDeclarationSyntax cls:
                LspCodeIndexModels.AddEntry(cls.Identifier.ValueText, "class", cls, scope, output, relativePath);
                scope.Push(cls.Identifier.ValueText);
                foreach (var member in cls.Members)
                {
                    CollectSymbols(member, scope, output, relativePath);
                }
                scope.Pop();
                break;
            case StructDeclarationSyntax st:
                LspCodeIndexModels.AddEntry(st.Identifier.ValueText, "struct", st, scope, output, relativePath);
                scope.Push(st.Identifier.ValueText);
                foreach (var member in st.Members)
                {
                    CollectSymbols(member, scope, output, relativePath);
                }
                scope.Pop();
                break;
            case InterfaceDeclarationSyntax iface:
                LspCodeIndexModels.AddEntry(iface.Identifier.ValueText, "interface", iface, scope, output, relativePath);
                scope.Push(iface.Identifier.ValueText);
                foreach (var member in iface.Members)
                {
                    CollectSymbols(member, scope, output, relativePath);
                }
                scope.Pop();
                break;
            case EnumDeclarationSyntax en:
                LspCodeIndexModels.AddEntry(en.Identifier.ValueText, "enum", en, scope, output, relativePath);
                foreach (var member in en.Members)
                {
                    LspCodeIndexModels.AddEntry(member.Identifier.ValueText, "enumMember", member, scope, output, relativePath);
                }
                break;
            case MethodDeclarationSyntax method:
                LspCodeIndexModels.AddEntry(method.Identifier.ValueText, "method", method, scope, output, relativePath);
                break;
            case ConstructorDeclarationSyntax ctor:
                LspCodeIndexModels.AddEntry(ctor.Identifier.ValueText, "constructor", ctor, scope, output, relativePath);
                break;
            case PropertyDeclarationSyntax property:
                LspCodeIndexModels.AddEntry(property.Identifier.ValueText, "property", property, scope, output, relativePath);
                break;
            case EventDeclarationSyntax ev:
                LspCodeIndexModels.AddEntry(ev.Identifier.ValueText, "event", ev, scope, output, relativePath);
                break;
            case EventFieldDeclarationSyntax ef:
                foreach (var variable in ef.Declaration.Variables)
                {
                    LspCodeIndexModels.AddEntry(variable.Identifier.ValueText, "event", variable, scope, output, relativePath);
                }
                break;
            case FieldDeclarationSyntax field:
                foreach (var variable in field.Declaration.Variables)
                {
                    LspCodeIndexModels.AddEntry(variable.Identifier.ValueText, "field", variable, scope, output, relativePath);
                }
                break;
            case DelegateDeclarationSyntax del:
                LspCodeIndexModels.AddEntry(del.Identifier.ValueText, "delegate", del, scope, output, relativePath);
                break;
        }
    }
}
