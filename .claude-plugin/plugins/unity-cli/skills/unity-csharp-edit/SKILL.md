---
name: unity-csharp-edit
description: Edit Unity C# code with unity-cli and LSP-backed write tools. Use when the user asks to rename symbols, replace method bodies, insert or remove members, create classes, or refactor scripts after inspecting code context. Do not use for read-only inspection; use the C# navigation skill when no code change is required.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: code
---

# C# Change Workflow

Plan and verify C# changes with unity-cli local tools, then apply file edits in the repository.
Read `references/lsp-write-safety.md` when you need preview-first edits, `namePath` guidance, or post-change verification steps.

## Use When

- The user wants to edit, create, rename, or refactor Unity C# scripts.
- The task can be expressed through symbol-aware LSP operations or `create_class`.
- The change should be validated with compilation state after it is applied.

## Do Not Use When

- The user only wants to inspect or explain existing code.
- The change is primarily about scenes, prefabs, or Unity object state rather than source files.

## Recommended Flow

1. Build or refresh the index, then inspect the target symbol with `read`, `get_symbols`, or `find_symbol`.
2. Preview the edit with `apply: false` whenever the tool supports it.
3. Apply one targeted mutation at a time with the appropriate LSP write tool or `create_class`.
4. Refresh the index for changed paths and check `get_compilation_state`.
5. If the edit is risky or touches multiple symbols, explain the expected scope before applying it.

## Commands

```bash
# Index
unity-cli raw build_index --json '{}'
unity-cli raw update_index --json '{"paths":["Assets/Scripts/Player.cs"]}'

# Locate target symbols before editing
unity-cli raw get_symbols --json '{"path":"Assets/Scripts/Player.cs"}'
unity-cli raw find_symbol --json '{"name":"Player","kind":"class","scope":"assets"}'
unity-cli raw find_refs --json '{"name":"Player","kind":"class","scope":"assets"}'

# Rename symbol (LSP)
unity-cli raw rename_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","newName":"Leap","apply":true}'

# Replace method body (LSP)
unity-cli raw replace_symbol_body --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","body":"{ Debug.Log(\"jump\"); }","apply":true}'

# Insert before/after symbol (LSP)
unity-cli raw insert_before_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","text":"public void Crouch() { }","apply":true}'
unity-cli raw insert_after_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","text":"public void Dash() { }","apply":true}'

# Remove symbol (LSP)
unity-cli raw remove_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","apply":true,"failOnReferences":true}'

# Validate C# text (LSP)
unity-cli raw validate_text_edits --json '{"relative":"Assets/Scripts/Player.cs","newText":"using UnityEngine;\npublic class Player : MonoBehaviour { }"}'

# Create a new class (local)
unity-cli raw create_class --json '{"name":"EnemyAI","namespace":"Game.AI","inherits":"MonoBehaviour","folder":"Assets/Scripts/AI"}'

# Read/search context
unity-cli raw read --json '{"path":"Assets/Scripts/Player.cs","startLine":1,"maxLines":120}'
unity-cli raw search --json '{"pattern":"public void Jump","path":"Assets/Scripts"}'

# Compilation
unity-cli raw get_compilation_state --json '{}'
```

## LSP Write Tools

All LSP write tools accept `apply` (boolean, default false):
- `apply: false` - preview mode, returns diff without modifying files
- `apply: true` - applies changes to disk

The `namePath` parameter navigates nested symbols with `/` separators:
- `"Player"` - top-level class
- `"Player/Jump"` - method Jump inside class Player
- `"Player/Config/MaxSpeed"` - field in nested type

## Examples

- "Rename `Player/Jump` to `Leap` everywhere it is safe to do so."
- "Insert a new method after `Player/Jump` and then check compilation."
- "Create a new `EnemyAI` MonoBehaviour under `Assets/Scripts/AI`."

## Common Issues

- `namePath` does not match the target: run `get_symbols` first and use the exact nested path.
- A removal may break references: inspect `find_refs` before calling `remove_symbol`.
- The edited file fails to compile: run `get_compilation_state` and fix the reported diagnostics before continuing.
- Preview-first edits are safer; use `apply: false` unless the change is trivial and well-scoped.
