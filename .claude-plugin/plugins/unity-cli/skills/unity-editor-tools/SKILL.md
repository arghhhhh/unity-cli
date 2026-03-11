---
name: unity-editor-tools
description: Inspect and control Unity Editor state with unity-cli. Use when the user asks to ping the editor, read console output, inspect or update project settings, run menu items, inspect windows or selection, manage packages, or capture profiler data. Do not use for scene creation, asset editing, or C# refactors that have their own dedicated skills.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: editor
---

# Editor Utilities & Profiler

Use this skill for editor-wide diagnostics and operations.
Read `references/editor-ops-checklist.md` when you need a safer sequence for settings, package, or profiler changes.

## Use When

- The user asks for editor health checks, console logs, or profiler data.
- The user wants to inspect or change project settings.
- The user wants to run a menu item, inspect windows, or manipulate current selection.
- The user wants package manager or registry operations from the editor side.

## Do Not Use When

- The task is primarily about a specific scene, prefab, asset, or script workflow.
- The user only needs local source inspection with no Unity Editor interaction.

## Recommended Flow

1. Start with `unity-cli system ping` and `get_editor_state` so later actions run against a known-good editor.
2. Read state before mutating it: console before clearing, settings before updating, profiler status before starting or stopping.
3. Apply one editor-wide change at a time and verify the result immediately.
4. When project settings or packages change, capture the before/after state in the response.

## Connection & Info

```bash
unity-cli system ping
unity-cli raw get_editor_info --json '{}'
unity-cli raw get_editor_state --json '{}'
unity-cli raw get_command_stats --json '{}'
```

## Project Settings

```bash
unity-cli raw get_project_settings --json '{"includePlayer":true}'
unity-cli raw update_project_settings --json '{"confirmChanges":true,"player":{"companyName":"MyStudio"}}'
```

## Editor Operations

```bash
unity-cli raw execute_menu_item --json '{"menuPath":"File/Save Project"}'
unity-cli raw manage_windows --json '{"action":"get"}'
unity-cli raw manage_selection --json '{"action":"get"}'
unity-cli raw manage_selection --json '{"action":"set","objectPaths":["/Player"]}'
unity-cli raw manage_tools --json '{"action":"get"}'
unity-cli raw quit_editor --json '{}'
```

## Console

```bash
unity-cli raw read_console --json '{"count":20}'
unity-cli raw clear_console --json '{}'
unity-cli raw clear_logs --json '{}'
```

## Profiler

```bash
unity-cli raw profiler_start --json '{}'
unity-cli raw profiler_stop --json '{}'
unity-cli raw profiler_status --json '{}'
unity-cli raw profiler_get_metrics --json '{"listAvailable":true}'
```

## Package Manager

```bash
unity-cli raw package_manager --json '{"action":"list"}'
unity-cli raw package_manager --json '{"action":"install","packageId":"com.unity.inputsystem"}'
unity-cli raw registry_config --json '{"action":"list"}'
```

## Examples

- "Show me the latest Unity console errors."
- "Update the company name in Project Settings."
- "Start the profiler, let it record briefly, then stop and report the status."

## Common Issues

- `system ping` fails: the Unity Editor bridge is not reachable, so stop before running settings or profiler operations.
- Console output is too noisy: add filters such as `count`, `filterText`, or `logTypes` before clearing anything.
- `update_project_settings` is rejected: include `"confirmChanges": true` and send only the sections that need to change.
- Profiler results are empty: query `profiler_status` first and use `profiler_get_metrics --json '{"listAvailable":true}'` before requesting specific metrics.
