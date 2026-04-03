---
version: 1.0.0
name: unity-input-system
description: Configure Unity Input System assets with unity-cli. Use when the user asks to create or remove action maps, add or remove actions and bindings, set up composite bindings, inspect input action assets, or manage control schemes. Do not use for runtime input simulation during tests; use the PlayMode testing skill for that workflow.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: input
---

# Input System Configuration

Manage Input Action Assets: action maps, actions, bindings, and control schemes.
Read `references/input-actions-playbook.md` when you need sequencing for action maps, composite bindings, or control scheme design.

## Use When

- The user wants to author or update `.inputactions` assets.
- The task is about action maps, actions, bindings, or control schemes.
- The user wants to inspect the structure of an input actions asset before modifying it.

## Do Not Use When

- The user wants to simulate keyboard, mouse, gamepad, or touch input during runtime tests.
- The task is only to verify whether the Input System package is installed.

## Recommended Flow

1. Decide which asset path and action map the change belongs to.
2. Create or inspect the action map before adding actions or bindings.
3. Add actions first, then bindings, then control schemes or composites.
4. Run `analyze_input_actions_asset` after non-trivial edits to catch structural issues.

## Action Maps

```bash
unity-cli raw create_action_map --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay"}'
unity-cli raw remove_action_map --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay"}'
```

## Actions

```bash
unity-cli raw add_input_action --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay","actionName":"Jump","actionType":"Button"}'
unity-cli raw remove_input_action --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay","actionName":"Jump"}'
```

## Bindings

```bash
unity-cli raw add_input_binding --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay","actionName":"Jump","path":"<Keyboard>/space"}'
unity-cli raw create_composite_binding --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay","actionName":"Move","compositeType":"2DVector","bindings":{"up":"<Keyboard>/w","down":"<Keyboard>/s","left":"<Keyboard>/a","right":"<Keyboard>/d"}}'
unity-cli raw remove_input_binding --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay","actionName":"Jump","bindingIndex":0}'
unity-cli raw remove_all_bindings --json '{"assetPath":"Assets/Input/Controls.inputactions","mapName":"Gameplay","actionName":"Jump"}'
```

## Analysis & Control

```bash
unity-cli raw analyze_input_actions_asset --json '{"assetPath":"Assets/Input/Controls.inputactions"}'
unity-cli raw get_input_actions_state --json '{}'
unity-cli raw manage_control_schemes --json '{"assetPath":"Assets/Input/Controls.inputactions","operation":"add","schemeName":"KeyboardMouse","devices":["Keyboard","Mouse"]}'
```

## Examples

- "Create a `Gameplay` action map with `Jump` and keyboard bindings."
- "Add a 2D movement composite to the `Move` action."
- "Inspect this input actions asset and tell me whether it is missing control schemes."

## Common Issues

- Bindings are added before the action exists: create the action first.
- Composite bindings are malformed: verify the `bindings` object contains all required parts.
- Runtime testing is needed after the asset change: switch to `unity-playmode-testing`.
- `actionType` values should be `Button`, `Value`, or `PassThrough`.
