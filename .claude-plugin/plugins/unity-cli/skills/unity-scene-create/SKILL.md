---
name: unity-scene-create
description: Create and bootstrap Unity scenes with unity-cli. Use when the user asks to create a new scene, load or save a scene, add starter GameObjects, or attach components while setting up a level or test scene. Do not use for editing existing objects in place or working inside prefab edit mode; use the dedicated editing or prefab skills for those cases.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: scenes
---

# Scene & GameObject Creation

Create scenes, GameObjects, and attach components via `unity-cli`.
Read `references/scene-bootstrap-patterns.md` when you need a safer scene setup order or a reminder of common starter patterns.

## Use When

- The user wants to create a new scene from scratch.
- The user asks to add initial objects and components while bootstrapping a scene.
- The user needs help loading, saving, or organizing a scene setup workflow.

## Do Not Use When

- The request is mainly about mutating existing objects in an already-prepared scene.
- The work is focused on prefab editing rather than scene authoring.

## Workflow

1. Create or load a scene
2. Create GameObjects (with optional primitive type)
3. Add components
4. Save the scene

## Commands

```bash
# Scene management
unity-cli scene create <name> --path Assets/Scenes/
unity-cli raw create_scene --json '{"sceneName":"MyScene","path":"Assets/Scenes/","loadScene":true}'
unity-cli raw load_scene --json '{"scenePath":"Assets/Scenes/MyScene.unity"}'
unity-cli raw save_scene --json '{"scenePath":"Assets/Scenes/MyScene.unity"}'

# GameObject creation
unity-cli raw create_gameobject --json '{"name":"Player","primitiveType":"Cube"}'
unity-cli raw create_gameobject --json '{"name":"Empty","parentPath":"/Canvas"}'

# Add components
unity-cli raw add_component --json '{"gameObjectPath":"/Player","componentType":"Rigidbody"}'
unity-cli raw add_component --json '{"gameObjectPath":"/Player","componentType":"BoxCollider"}'
```

## Examples

- "Create a new gameplay scene and add a `Player` object with physics components."
- "Load `Assets/Scenes/TestScene.unity`, add a camera rig, then save it."
- "Bootstrap a simple empty scene for UI testing."

## Common Issues

- The scene is created but not persisted: call `save_scene` after bulk changes.
- Objects end up in the wrong place: set `parentPath` explicitly during creation.
- Primitive setup is inconsistent: use supported `primitiveType` values such as `cube`, `sphere`, `capsule`, `cylinder`, `plane`, or `quad`.
- If the task turns into modifying existing objects rather than creating them, switch to `unity-gameobject-edit`.
