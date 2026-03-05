# tasks/todo.md

このファイルは、非自明タスクの計画・進捗・検証結果を記録する作業ノート。

## Current Task

- Title: CLI 引数仕様の明示化と実行前バリデーション強化
- Request Date: 2026-03-06
- Owner: Codex
- Scope: `tool schema` 追加、`--dry-run` 導入、JSON Schema 検証（`oneOf`/`anyOf` 対応）、action別必須を含む主要ツールの厳格スキーマ化、Issue-first Spec/TDD 運用へのドキュメント・テンプレート整合
- Spec: Issue-first 運用（ローカル `specs/SPEC-*` は新規作成しない）

## Plan

- [x] Step 1: 既存 CLI 引数・ツールカタログの調査と方針整理
- [x] Step 2: `tool schema` サブコマンドと `ToolSpec` 公開 API の実装
- [x] Step 3: `--dry-run` で mutating ツールをスキップし結果を返す動作を実装
- [x] Step 4: パラメータ検証器を実装し、`type`/`required`/`additionalProperties`/`enum`/`oneOf`/`anyOf` を実行前に検証
- [x] Step 5: 主要ツール群の `params_schema` を strict 化し、回帰テストを追加
- [x] Step 6: README / docs 更新と品質ゲート通過確認
- [x] Step 7: Spec/TDD テンプレートと Speckit 注意書きを Issue-first 方針へ整合

## Verification

- [x] `cargo fmt --all` — pass
- [x] `cargo clippy --all-targets -- -D warnings` — pass
- [x] `cargo test --all-targets` — 199 tests pass
- [ ] `dotnet test lsp/Server.Tests.csproj` — `dotnet` command not found
- [x] `cargo run -- tool schema create_scene --output json` — schema 出力確認
- [x] `cargo run -- --dry-run tool call create_scene --json '{"sceneName":"PreviewScene"}' --output json` — dry-run スキップ応答確認

## Review

- Summary: CLI で受け取る引数仕様を機械可読化し、実行前に厳格検証することでエージェント向けの予測可能性を向上。`tool schema` と `--dry-run` に加え、action別必須を `oneOf` で検証し、エラー時は action に一致する詳細（例: `$.layerName is required`）を返す運用を導入。
- Risks: action ごとの条件付き必須は主要ツールから段階適用のため、未適用ツールは継続的な厳格化が必要。
- Follow-ups: `if/then` 相当のバリデーションを追加して、`oneOf` エラーを項目単位で分かりやすくする。

## History

- 2026-02-27: CLAUDE.md 運用強化 / `tasks/*.md` 作成と参照追記を完了
- 2026-03-02: Issue #54 パス不整合修正 — 3ファイル修正完了、Rust 品質ゲート全通過
- 2026-03-06: CLI 引数仕様強化 — `tool schema` / `--dry-run` / strict schema validation + action別必須検証 実装、Issue-first Spec/TDD 運用に docs+templates+constitution mirror を整合、199 tests pass
