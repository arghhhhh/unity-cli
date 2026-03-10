---
name: unity-scene-inspect
description: Inspect Unity scenes without mutating them. Use when the user asks to analyze scene hierarchy, list scenes, find GameObjects by name or component, inspect component values, review object references, or query animator state. Do not use for creating scenes or editing GameObjects; use the creation or editing skills for those workflows.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: scenes
---

# Scene Analysis & Navigation

Inspect scene hierarchy, find objects, and read component data.
Read `references/scene-inspection-playbook.md` when the scene is large, object names are duplicated, or you need a step-by-step inspection flow.

## Use When

- The user wants to inspect the current scene before making changes.
- The user asks where a GameObject, component, or animator state lives.
- The user wants a hierarchy summary, scene inventory, or reference analysis.

## Do Not Use When

- The task is to create new scenes or modify existing objects.
- The user only needs source-code analysis with no scene context.

## Recommended Flow

1. Start with `get_scene_info`, `list_scenes`, or `get_hierarchy` to understand the current scope.
2. Locate candidate objects with `find_gameobject` or `find_by_component`.
3. Inspect the chosen target with `get_gameobject_details`, `get_component_values`, or animator queries.
4. Use `analyze_scene_contents` for broad audits or when inactive objects matter.

## Commands

```bash
# Hierarchy & scene info
unity-cli raw get_hierarchy --json '{"nameOnly":true}'
unity-cli raw get_scene_info --json '{}'
unity-cli raw list_scenes --json '{}'

# Find objects
unity-cli raw find_gameobject --json '{"name":"Player"}'
unity-cli raw find_by_component --json '{"componentType":"Camera"}'

# Inspect details
unity-cli raw get_gameobject_details --json '{"gameObjectName":"Player"}'
unity-cli raw get_component_values --json '{"gameObjectName":"Player","componentType":"Transform"}'
unity-cli raw get_object_references --json '{"gameObjectName":"Player"}'

# Scene analysis
unity-cli raw analyze_scene_contents --json '{"includeInactive":true}'

# Animator
unity-cli raw get_animator_state --json '{"gameObjectName":"Player"}'
unity-cli raw get_animator_runtime_info --json '{"gameObjectName":"Player"}'
```

## Examples

- "Show me the current scene hierarchy and find the player camera."
- "Inspect the `Transform` values on `Player`."
- "Tell me which animator state the character is currently in."

## Common Issues

- The scene is large: start with `get_hierarchy --json '{"nameOnly":true}'` and then narrow down.
- Multiple objects share the same name: use details and references to disambiguate before reporting results.
- Inactive objects are missing from audits: use `analyze_scene_contents --json '{"includeInactive":true}'`.
- These tools are read-only; switch to the creation or editing skill before making mutations.
