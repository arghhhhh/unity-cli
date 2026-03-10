---
name: unity-ui-automation
description: Automate Unity UI inspection and interaction with unity-cli. Use when the user asks to find UI elements by name or type, inspect button or input state, click UI elements, set UI values, or run short UI interaction sequences during testing. Do not use for scene creation or general PlayMode control when UI-specific targeting is not needed.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: ui
---

# UI Element Automation

Find, inspect, and interact with UI elements (uGUI / UI Toolkit).
Read `references/ui-test-flow.md` when you need a safer locate-inspect-interact sequence for UI testing.

## Use When

- The user wants to locate UI elements by name or type.
- The task requires inspecting UI state before or after interaction.
- The user wants to click UI elements, set values, or run a short UI input sequence.

## Do Not Use When

- The task is general PlayMode setup with no UI targeting requirement.
- The request is about editing UI prefabs or scene hierarchy rather than testing interactions.

## Recommended Flow

1. Find the target UI element with `namePattern` or `elementType`.
2. Inspect its current state before acting, especially for visibility and interactability.
3. Perform one interaction at a time and re-check state when the sequence matters.
4. Combine with PlayMode control only when the UI depends on runtime state.

## Commands

```bash
# Find UI elements
unity-cli raw find_ui_elements --json '{"namePattern":"Start","includeInactive":true}'
unity-cli raw find_ui_elements --json '{"elementType":"Button","includeInactive":true}'

# Inspect state
unity-cli raw get_ui_element_state --json '{"elementPath":"/Canvas/StartButton"}'

# Interact
unity-cli raw click_ui_element --json '{"elementPath":"/Canvas/StartButton"}'
unity-cli raw set_ui_element_value --json '{"elementPath":"/Canvas/NameInput","value":"Player1"}'
unity-cli raw simulate_ui_input --json '{"inputSequence":[{"type":"setvalue","params":{"elementPath":"/Canvas/Slider","value":"0.75"}}],"waitBetween":50}'
```

## Examples

- "Find the start button and click it."
- "Read the current value of `/Canvas/NameInput` and then set it to `Player1`."
- "Run a short slider interaction sequence in the active UI."

## Common Issues

- The element is not found: narrow with `namePattern` or `elementType`, and include inactive elements when needed.
- The click has no effect: inspect `get_ui_element_state` first to verify visibility and interactability.
- A longer runtime test is needed: combine with `unity-playmode-testing` for Play Mode control.
- Use `simulate_ui_input` for compact repeated interactions rather than many one-off commands.
