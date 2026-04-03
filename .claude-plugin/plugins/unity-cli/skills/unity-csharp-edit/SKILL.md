---
version: 1.0.0
name: unity-csharp-edit
description: Implement, fix, and refactor Unity C# code with unity-cli write tools. Use when the user wants to change behavior in .cs files, create or rewrite scripts, update multiple C# files together, rename symbols, add or remove members, or change project or package settings as part of a code change. Prefer this skill whenever the request implies writing Unity C# code; do not use it for read-only inspection with no planned edit.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.3.0
  category: code
---

# Unity C# Editing

Implement Unity C# changes with unity-cli as the primary write path.
Read references only when the change needs them:

- `references/file-write-recipes.md` for choosing symbol edits, file writes, and multi-file writes.
- `references/lsp-write-safety.md` for preview-first edits, `namePath`, and write result checks.
- `references/unity-implementation-guidelines.md` before touching serialized fields, UI bindings, editor code, ScriptableObjects, or SettingsProviders.
- `references/settings-write-recipes.md` when the task includes project or package settings.

## Critical Rules

- Use this skill when the user intends to change code. Use `unity-csharp-navigate` only for read-only investigation.
- Prefer the smallest write primitive that keeps the change correct.
- Preserve Unity serialization, binding paths, and editor/runtime boundaries when refactoring.
- Do not add defensive `null` checks unless a real nullable contract, Unity lifecycle gap, or external input boundary requires them.
- After non-trivial writes, confirm `get_compilation_state` and address real diagnostics before unrelated follow-up work.

## Recommended Flow

1. Decide the write shape first: symbol edit, full-file write, multi-file write, or settings change.
2. Read only the minimum context needed with `read`, `get_symbols`, `find_symbol`, or `find_refs`.
3. Preview risky symbol edits with `apply: false`.
4. Apply the write and opt into `refresh`, `waitForCompile`, and `updateIndex` when later steps depend on clean diagnostics or fresh symbol data.
5. Inspect `changedFiles`, `changedSymbols`, and `diagnostics`. Fix compilation issues before moving on.

## Symbol-Aware Edits

```bash
unity-cli raw get_symbols --json '{"path":"Assets/Scripts/Player.cs"}'
unity-cli raw find_symbol --json '{"name":"Player","kind":"class","scope":"assets"}'
unity-cli raw find_refs --json '{"name":"Player","kind":"class","scope":"assets"}'

unity-cli raw rename_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","newName":"Leap","apply":false}'
unity-cli raw replace_symbol_body --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","body":"{ velocity.y = jumpSpeed; }","apply":true}'
unity-cli raw insert_before_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","text":"[SerializeField] private float dashCooldown = 0.25f;","apply":true}'
unity-cli raw insert_after_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/Jump","text":"private bool CanDash() { return dashCooldown > 0f; }","apply":true}'
unity-cli raw remove_symbol --json '{"relative":"Assets/Scripts/Player.cs","namePath":"Player/LegacyJump","apply":true,"failOnReferences":true}'
```

## File-Level Writes

```bash
unity-cli raw write_csharp_file --json '{"relative":"Assets/Scripts/PlayerController.cs","newText":"using UnityEngine;\n\npublic sealed class PlayerController : MonoBehaviour\n{\n    [SerializeField] private float speed = 5f;\n}\n","validate":true,"apply":true,"format":true,"waitForCompile":true,"updateIndex":true}'
unity-cli raw create_csharp_file --json '{"relative":"Assets/Tests/EditMode/PlayerControllerTests.cs","text":"using NUnit.Framework;\n\npublic class PlayerControllerTests {}\n","validate":true,"apply":true,"waitForCompile":true,"updateIndex":true}'
unity-cli raw apply_csharp_edits --json '{"files":[{"relative":"Assets/Scripts/IPlayerMover.cs","newText":"public interface IPlayerMover { void Move(); }\n"},{"relative":"Assets/Scripts/PlayerMover.cs","newText":"public sealed class PlayerMover : IPlayerMover { public void Move() {} }\n"}],"validate":true,"apply":true,"waitForCompile":true,"updateIndex":true}'
unity-cli raw create_class --json '{"name":"EnemyAI","namespace":"Game.AI","inherits":"MonoBehaviour","folder":"Assets/Scripts/AI"}'
```

## Validation And Follow-Up

```bash
unity-cli raw validate_text_edits --json '{"relative":"Assets/Scripts/PlayerController.cs","newText":"using UnityEngine;\npublic sealed class PlayerController : MonoBehaviour {}\n"}'
unity-cli raw update_index --json '{"paths":["Assets/Scripts/PlayerController.cs","Assets/Tests/EditMode/PlayerControllerTests.cs"]}'
unity-cli raw get_compilation_state --json '{}'
```

## Settings

```bash
unity-cli raw get_project_setting --json '{"path":"Player/activeInputHandler"}'
unity-cli raw set_project_setting --json '{"path":"Player/activeInputHandler","value":"InputSystemPackage","confirmChanges":true}'
unity-cli raw get_project_settings --json '{"includePlayer":true,"includeEditor":true}'
unity-cli raw get_package_setting --json '{"package":"com.unity.testtools.codecoverage","key":"EnableCodeCoverage","scope":"project"}'
unity-cli raw set_package_setting --json '{"package":"com.unity.testtools.codecoverage","key":"EnableCodeCoverage","value":true,"scope":"project","confirmChanges":true}'
```

## Examples

- "Implement dash cooldown in `PlayerController.cs` and update the related tests."
- "Rewrite this UXML-bound view model without breaking serialized field names or binding paths."
- "Create a new editor utility script and change the matching project or package setting."
- "Refactor an interface, its implementation, and the tests in one coordinated write."

## Common Issues

- `namePath` does not match the target: run `get_symbols` first and use the exact nested path.
- A rename breaks Unity serialization or UI binding: check `[SerializeField]` names, UXML binding paths, and reflection-based lookups before renaming.
- A coordinated change is spread across repeated symbol edits: switch to `apply_csharp_edits` or `write_csharp_file`.
- The write result looks successful but the project is still broken: inspect `diagnostics`, then run `get_compilation_state` and fix the actual errors.
