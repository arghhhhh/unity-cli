---
version: 1.0.0
name: unity-gameobject-edit
description: Edit existing GameObjects and components in Unity with unity-cli. Use when the user asks to rename objects, move or deactivate them, change component fields, add or remove tags and layers, or delete objects or components in an existing scene. Do not use for new scene bootstrapping or prefab edit mode; those have dedicated skills.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: scenes
---

# GameObject & Component Editing

Modify existing GameObjects and their components.
Read `references/component-edit-safety.md` when the task involves destructive edits, nested field paths, or tag/layer changes.

## Use When

- The user wants to change an existing object's properties or hierarchy.
- The user wants to inspect, modify, or remove components on an existing object.
- The user needs tag or layer management in the current project.

## Do Not Use When

- The task is to create a brand new scene layout from scratch.
- The task is specifically about prefab asset editing rather than scene objects.

## Recommended Flow

1. Identify the exact `gameObjectPath` and current components before changing anything.
2. Apply one mutation at a time with `modify_gameobject`, `modify_component`, or field-level edits.
3. Use destructive commands like `delete_gameobject` or `remove_component` only after confirming scope.
4. Save the scene after destructive or bulk updates.

## Critical: Parameter Name Reference

Different tools use different parameter names. Using the wrong one causes errors:
- **Read-only** (`get_gameobject_details`, `get_component_values`): use `gameObjectName`
- **Mutations** (`add_component`, `modify_component`, `set_component_field`, `remove_component`, `list_components`): use `gameObjectPath`
- **GameObject mutations** (`modify_gameobject`, `delete_gameobject`): use `path`

When unsure: `unity-cli tool schema <tool_name> --output json`

## Commands

```bash
# Modify GameObject properties (uses "path", NOT "gameObjectName" or "gameObjectPath")
unity-cli raw modify_gameobject --json '{"path":"/Player","name":"Hero","active":true}'
unity-cli raw delete_gameobject --json '{"path":"/OldObject"}'

# Component operations (uses "gameObjectPath", NOT "gameObjectName")
unity-cli raw modify_component --json '{"gameObjectPath":"/Player","componentType":"Rigidbody","properties":{"mass":2.0}}'
unity-cli raw set_component_field --json '{"gameObjectPath":"/Player","componentType":"Transform","fieldPath":"position","value":{"x":0,"y":1,"z":0}}'
unity-cli raw remove_component --json '{"gameObjectPath":"/Player","componentType":"BoxCollider"}'
unity-cli raw list_components --json '{"gameObjectPath":"/Player"}'
unity-cli raw get_component_types --json '{}'

# Tags & layers
unity-cli raw manage_tags --json '{"action":"add","tagName":"Enemy"}'
unity-cli raw manage_layers --json '{"action":"get"}'
```

## Examples

- "Deactivate `/Player` and rename it to `Hero`."
- "Change the Rigidbody mass on `/Player` and move it to a new position."
- "Remove the old collider from `/Obstacle` after checking its components."

## Common Issues

- `gameObjectPath` is wrong: inspect the hierarchy first and use the full path.
- `set_component_field` fails: use dot-separated `fieldPath` values and verify the target component exists.
- Destructive edits removed too much: prefer listing components or inspecting the object before delete operations.
- Save the scene after destructive edits so changes do not vanish on reload.
