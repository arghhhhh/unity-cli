---
version: 1.0.0
name: unity-playmode-testing
description: Drive Unity runtime verification with unity-cli. Use when the user asks to enter or exit Play Mode, run EditMode or PlayMode tests, simulate keyboard, mouse, gamepad, or touch input, capture screenshots or video, or inspect current test status. Do not use for authoring input action assets; use the Input System skill for that.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: testing
---

# PlayMode, Testing & Input Simulation

Control Play Mode, run tests, simulate input devices, and capture media.
Read `references/playmode-test-loop.md` when you need a clean execution loop for entering Play Mode, sending input, waiting for results, and capturing evidence.

## Use When

- The user wants to run EditMode or PlayMode tests.
- The task requires runtime input simulation.
- The user wants screenshots or video from the current game or editor state.
- The user wants to inspect current test progress or runtime state.

## Do Not Use When

- The task is about editing input action assets rather than runtime behavior.
- The request is purely about static scene or code inspection.

## Recommended Flow

1. Confirm editor state before entering Play Mode or running tests.
2. Enter Play Mode or start tests, then wait until the runtime is ready before sending input.
3. Capture screenshots or short video only after the target state is visible.
4. Stop Play Mode or recording cleanly and report the final status.

## Play Control

```bash
unity-cli raw play_game --json '{}'
unity-cli raw pause_game --json '{}'
unity-cli raw stop_game --json '{}'
unity-cli raw get_editor_state --json '{}'
```

## Input Simulation

```bash
unity-cli raw input_keyboard --json '{"key":"space","action":"press"}'
unity-cli raw input_mouse --json '{"action":"click","button":"left","x":400,"y":300}'
unity-cli raw input_gamepad --json '{"action":"button","button":"a","buttonAction":"press"}'
unity-cli raw input_touch --json '{"action":"tap","x":200,"y":400}'
unity-cli raw create_input_sequence --json '{"sequence":[{"type":"keyboard","params":{"action":"press","key":"space"}}],"delayBetween":100}'
unity-cli raw get_current_input_state --json '{}'
```

## Screenshots & Video

```bash
unity-cli raw capture_screenshot --json '{"captureMode":"game","width":1280,"height":720}'
unity-cli raw analyze_screenshot --json '{"imagePath":"Assets/Screenshots/test.png"}'
unity-cli raw capture_video_start --json '{"captureMode":"game","fps":30,"maxDurationSec":5}'
unity-cli raw capture_video_stop --json '{}'
unity-cli raw capture_video_status --json '{}'
```

## Testing

```bash
unity-cli raw run_tests --json '{"testMode":"PlayMode"}'
unity-cli raw run_tests --json '{"testMode":"EditMode","filter":"PlayerTests"}'
unity-cli raw get_test_status --json '{}'
```

## Examples

- "Run PlayMode tests for the player flow and report the result."
- "Enter Play Mode, press space, and capture a screenshot."
- "Record a 5 second gameplay clip and stop automatically."

## Common Issues

- Input is ignored: wait until Play Mode is fully active before sending input.
- Long recordings never stop: prefer `maxDurationSec` for unattended captures.
- Test scope is too broad: use `filter` or a single `testMode` before running the suite.
- `testMode` values are `EditMode`, `PlayMode`, or `All`.
