# tasks/todo.md

このファイルは、非自明タスクの計画・進捗・検証結果を記録する作業ノート。

## Current Task

- Title: Issue #107 C# 編集ワークフロー強化
- Request Date: 2026-03-10
- Owner: Codex
- Scope: issue #107 の Spec/Plan/Tasks/TDD 更新、write 系の default LSP 化、共通レスポンス contract、`namePath` 整合、`write_csharp_file` / `create_csharp_file` / `apply_csharp_edits`、post-write pipeline、project/package setting API、関連テスト
- Spec: GitHub Issue #107（`gwt-spec` ラベル、ローカル `specs/SPEC-*` は新規作成しない）

## Plan

- [x] Step 1: issue #107 を `gwt-spec` 化し、Spec/Plan/Tasks/TDD と進捗コメントを更新
- [x] Step 2: write 系の default LSP 化、共通レスポンス contract、`namePath` 正規化を実装
- [x] Step 3: `write_csharp_file` / `create_csharp_file` / `apply_csharp_edits` と post-write pipeline を実装
- [x] Step 4: `get_project_setting` / `set_project_setting` / `get_package_setting` / `set_package_setting` を実装
- [x] Step 5: Rust / LSP / Unity bridge のテストを追加し、品質ゲートを実行

## Verification

- [x] `cargo fmt --all -- --check` 相当 (`cargo fmt --all` 実行後に差分なし)
- [x] `cargo clippy --all-targets -- -D warnings`
- [x] `cargo test --all-targets` — 202 tests pass
- [x] `dotnet test lsp/Server.Tests.csproj` — 3 tests pass (`.cache/dotnet/sdk` のローカル SDK を使用)
- [x] `dotnet build UnityCliBridge/UnityCliBridge.Editor.csproj` — success
- [x] `dotnet build UnityCliBridge/UnityCliBridge.Tests.csproj` — success
- [x] Unity batchmode EditMode tests — `BridgeBatchEditModeTestRunner` で `145 total / 139 passed / 0 failed / 6 skipped` を確認。skip はすべて batchmode で listener を立てない integration tests

## Review

- Summary: issue #107 を `gwt-spec` 化した上で、C# write 系の default LSP 化、共通レスポンス contract、`namePath` 整合、validated file write/create/multi-file edit、post-write pipeline、singular settings API を追加。Rust 202 tests、LSP `dotnet test` 3 tests、Unity generated csproj build、Unity batch EditMode `145 total / 139 passed / 0 failed / 6 skipped` を確認。
- Risks: `get_package_setting` / `set_package_setting` は repo に `SettingsManagement` 依存が無いため、project/user JSON ストアで機能を成立させている。Unity 公式 `-runTests -testResults ...` はこの project では依然として指定先 XML を吐かず、検証は `TestExecutionHandler` 経由の batch helper で補完した。
- Follow-ups: Unity 公式 command-line test runner の `testResults` 出力不整合を別 issue で切り分ける。必要なら `manifest.json` の `testables` 運用と package test discovery フローを明文化する。

## History

- 2026-02-27: CLAUDE.md 運用強化 / `tasks/*.md` 作成と参照追記を完了
- 2026-03-02: Issue #54 パス不整合修正 — 3ファイル修正完了、Rust 品質ゲート全通過
- 2026-03-06: CLI 引数仕様強化 — `tool schema` / `--dry-run` / strict schema validation + action別必須検証 実装、Issue-first Spec/TDD 運用に docs+templates+constitution mirror を整合、199 tests pass
- 2026-03-10: Issue #107 C# 編集ワークフロー強化 — gwt-spec 化、C# write contract/API/settings API 実装、Rust 202 tests pass、`dotnet test` 3 tests pass、Unity generated csproj build success、Unity batch EditMode `145 total / 139 passed / 0 failed / 6 skipped`
