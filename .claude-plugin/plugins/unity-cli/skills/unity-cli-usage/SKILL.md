---
name: unity-cli-usage
description: Use unity-cli as the default interface for Unity Editor automation and local code tools. Use when the user asks how to verify the CLI, connect to Unity, switch active instances, choose between typed commands and raw tools, or troubleshoot unity-cli setup. Do not use once a more specific scene, asset, code, or testing skill clearly matches the task.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: foundation
---

# unity-cli Usage

Use `unity-cli` as the primary Unity automation interface.
Read `references/runtime-checklist.md` when installation mode, instance selection, or CI environment details matter.

## Use When

- The user asks how to connect Claude Code to Unity through `unity-cli`.
- The user needs help with `system ping`, `instances list`, `instances set-active`, or `--output json`.
- The user asks whether to use a typed subcommand or `raw`.
- The task is blocked on CLI setup, host/port selection, or connection troubleshooting.

## Do Not Use When

- A more specific Unity workflow skill already covers the requested task.
- The user only wants to inspect or edit project files without using `unity-cli`.

## Recommended Flow

1. Verify whether `unity-cli` is available globally or must be run with `cargo run --`.
2. Confirm the target Unity instance with `system ping` or `instances list`.
3. Prefer typed subcommands such as `system`, `scene`, or `instances` when available.
4. Fall back to `raw` only when there is no typed command for the needed tool.
5. Use `--output json` for chained automation or when another tool will consume the result.

## Bootstrap

```bash
if ! command -v unity-cli >/dev/null 2>&1; then
  if [ -f Cargo.toml ] && grep -q '^name = "unity-cli"' Cargo.toml; then
    echo "unity-cli is not installed globally. Use: cargo run -- <args>"
  else
    echo "unity-cli is not installed."
    echo "Install a release binary from https://github.com/akiojin/unity-cli/releases"
    echo "or clone the repo and run: cargo install --path ."
    exit 1
  fi
fi
```

Then verify:

```bash
unity-cli --version
```

## Preferred Order

1. Use typed subcommands (`system`, `scene`, `instances`) when available.
2. Use `raw` for the rest of Unity command types.
3. Use `--output json` when chaining automation steps.

## Commands

```bash
unity-cli system ping
unity-cli scene create MainScene --path Assets/Scenes/
unity-cli raw create_gameobject --json '{"name":"Player"}'
unity-cli instances list --ports 6400,6401
unity-cli instances set-active localhost:6401
```

## Examples

- "Check whether `unity-cli` can reach my Unity Editor."
- "Switch to the Unity instance running on port 6401."
- "Tell me whether this workflow should use `scene create` or a raw tool call."

## Common Issues

- `unity-cli` not found: use `cargo run -- <args>` from the repo root or install the release binary.
- `instances set-active` returns `unreachable`: run `instances list` and select a target with `up` status.
- CI or remote runs target the wrong editor: set `UNITY_CLI_HOST` and `UNITY_CLI_PORT` explicitly.
- Raw payloads fail: make sure `--json` receives a valid JSON object, not shell-expanded fragments.
