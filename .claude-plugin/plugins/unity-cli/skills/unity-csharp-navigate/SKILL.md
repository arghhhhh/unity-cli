---
version: 1.0.0
name: unity-csharp-navigate
description: Explore Unity C# code without modifying files. Use when the user asks to read scripts, search text, find symbols, trace references, inspect namespaces or packages, or understand where a class, method, or field is used. Do not use for code edits, renames, or refactors; use the editing skill for those.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: code
---

# C# Code Exploration

Navigate and search C# source using the unity-cli local tools.
Read `references/code-search-playbook.md` when you need indexing strategy, path narrowing, or reference tracing guidance.

## Use When

- The user wants to inspect a script or understand existing C# behavior.
- The user asks where a symbol is defined or referenced.
- The user wants to search packages or source trees before making a change.

## Do Not Use When

- The user wants to modify code or create new C# files.
- The task depends on Unity scene state rather than source files.

## Recommended Flow

1. Build or refresh the index before symbol-heavy queries.
2. Start with `read` or `search` to anchor on the right file or path.
3. Use `get_symbols`, `find_symbol`, and `find_refs` once the target identifier is clear.
4. Narrow the search path or page size if the result set is large.
5. Use `list_packages` when the question might involve packages rather than project sources.

## Commands

```bash
# Read source files
unity-cli raw read --json '{"path":"Assets/Scripts/Player.cs"}'
unity-cli raw read --json '{"path":"Assets/Scripts/Player.cs","startLine":10,"maxLines":20}'

# Search code
unity-cli raw search --json '{"pattern":"OnCollisionEnter","path":"Assets/Scripts"}'

# Symbol navigation
unity-cli raw get_symbols --json '{"path":"Assets/Scripts/Player.cs"}'
unity-cli raw find_symbol --json '{"name":"PlayerController","kind":"class","scope":"assets"}'
unity-cli raw find_refs --json '{"name":"Health","scope":"assets","pageSize":20}'

# Index management
unity-cli raw build_index --json '{}'

# Packages
unity-cli raw list_packages --json '{}'
```

## Examples

- "Read `PlayerController.cs` and explain how jumping works."
- "Find every reference to `Health` in project scripts."
- "List installed packages and check whether Input System is present."

## Common Issues

- Symbol lookups return nothing: run `unity-cli raw build_index --json '{}'` or `update_index` for the changed path.
- Results are too broad: use `path` in `search` or `pageSize` in `find_refs`.
- The user needs an actual code change: hand off to `unity-csharp-edit` after gathering context.
- These tools work locally; a Unity Editor connection is not required.
