---
version: 1.0.0
name: unity-addressables
description: Manage Unity Addressables groups and content with unity-cli. Use when the user asks to list or create groups, add or remove entries, build or clean Addressables content, or analyze and fix Addressables issues before a build. Do not use for generic asset import settings or material editing; use asset management for those tasks.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: assets
---

# Addressables Operations

Manage Addressable Asset groups, build content, and analyze bundles.
Read `references/addressables-build-loop.md` when you need a safer order for group changes, analysis, and clean builds.

## Use When

- The user wants to manage Addressables groups or entries.
- The task involves building or cleaning Addressables content.
- The user wants an analysis pass or automatic issue fix before shipping content.

## Do Not Use When

- The task is about general asset import settings or dependency analysis outside Addressables.
- The request is about scene object setup rather than content delivery.

## Recommended Flow

1. Inspect existing groups before creating or moving entries.
2. Apply group or entry changes with `addressables_manage`.
3. Run `addressables_analyze` before a build, and use `fix_issues` only when the reported changes are acceptable.
4. Build with `addressables_build`, using `clean_build` when structure changed substantially.

## Commands

```bash
# Group management
unity-cli raw addressables_manage --json '{"action":"list_groups"}'
unity-cli raw addressables_manage --json '{"action":"create_group","groupName":"Characters"}'
unity-cli raw addressables_manage --json '{"action":"add_entry","groupName":"Characters","assetPath":"Assets/Prefabs/Hero.prefab","address":"hero"}'
unity-cli raw addressables_manage --json '{"action":"remove_entry","address":"hero"}'

# Build
unity-cli raw addressables_build --json '{"action":"build"}'
unity-cli raw addressables_build --json '{"action":"clean_build"}'

# Analysis
unity-cli raw addressables_analyze --json '{"action":"analyze"}'
unity-cli raw addressables_analyze --json '{"action":"fix_issues"}'
```

## Examples

- "Create an Addressables group for character prefabs and add the hero prefab."
- "Analyze Addressables issues and then build content."
- "Run a clean Addressables build after reorganizing groups."

## Common Issues

- The wrong group or address is targeted: list groups first and verify the entry path.
- Builds fail after structural changes: run `analyze` and prefer `clean_build`.
- `fix_issues` can be broad: inspect the analysis result before applying it in a risky project.
- Generic asset problems should go through `unity-asset-management`, not this skill.
