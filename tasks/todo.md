# tasks/todo.md

このファイルは、非自明タスクの計画・進捗・検証結果を記録する作業ノート。

## Current Task

- Title: 動画・静止画・Profiler 中心のパフォーマンス計測/改善基盤
- Request Date: 2026-03-11
- Owner: Codex
- Scope: bridge の `get_command_stats` 拡張、capture/profiler の stage timing 計測、`.unity` 出力時の不要 refresh 削減、generated perf scene 生成、scenario 切替 controller、ローカル perf benchmark script、関連テスト/検証
- Spec: ローカル計画ベース。GitHub Issue-first には未昇格

## Plan

- [x] Step 1: `get_command_stats` を per-command timing/stage breakdown 付きに拡張する
- [x] Step 2: screenshot/video/profiler handlers に stage timing を追加し、`.unity` 出力時の不要 AssetRefresh を削減する
- [x] Step 3: generated perf scene と runtime scenario controller を追加する
- [x] Step 4: 動画・静止画・Profiler をまとめて回す perf benchmark script を追加する
- [ ] Step 5: Rust / Unity / ローカル benchmark の検証を実行し、結果を記録する

## Verification

- [x] `cargo fmt --all -- --check`
- [x] `cargo clippy --all-targets -- -D warnings`
- [x] `cargo test --all-targets`
- [x] `dotnet test lsp/Server.Tests.csproj`
- [x] `bash -n scripts/perf-media-benchmark.sh`
- [x] `cargo check -q`
- [ ] `dotnet build UnityCliBridge/UnityCliBridge.Editor.csproj` — workspace 上に csproj が無く未実施
- [ ] `dotnet build UnityCliBridge/UnityCliBridge.Tests.csproj` — workspace 上に csproj が無く未実施
- [ ] Unity Editor 接続下で perf benchmark script を 1 回実行 — `system ping` / `get_command_stats` / `get_compilation_state` が response header timeout で未実施
- [x] 暫定 runtime 計測 — 2026-03-12 に稼働中の別 Unity Editor project (`feature/input-system`, `127.0.0.1:6401`) で `SampleScene` を使い、`load_scene 785ms / play_game 787ms / profiler_start 722ms / capture_screenshot 724ms / capture_video_start 1014ms / capture_video_stop 701ms / profiler_stop 612ms / get_command_stats 672ms` を確認

## Review

- Summary: `get_command_stats` を `counts` / `recent` / `timings` 付きの bridge-side snapshot に拡張し、capture_screenshot / capture_video_* / profiler_* で stage timing を記録するようにした。合わせて Generated/E2E media perf scene、scenario controller、`.unity/perf-media/` 出力の benchmark script を追加した。
- Risks: Unity Editor 側が現在 response header timeout で応答停止しており、perf benchmark 本体と Unity package compile の実機確認は未完了。`scripts/perf-media-benchmark.sh` は `summary.md` / `result.json` を出す実装だが、実行実績はまだ取れていない。
- Follow-ups: current `feature/performance/UnityCliBridge` の Editor instance で bridge listener が起動しない原因を切り分ける。復帰後に 1. `get_compilation_state` 2. `get_command_stats` 3. `scripts/perf-media-benchmark.sh` を順に実行し、暫定値を current branch 上の artifact で置き換える。

## History

- 2026-02-27: CLAUDE.md 運用強化 / `tasks/*.md` 作成と参照追記を完了
- 2026-03-02: Issue #54 パス不整合修正 — 3ファイル修正完了、Rust 品質ゲート全通過
- 2026-03-06: CLI 引数仕様強化 — `tool schema` / `--dry-run` / strict schema validation + action別必須検証 実装、Issue-first Spec/TDD 運用に docs+templates+constitution mirror を整合、199 tests pass
- 2026-03-10: Issue #107 C# 編集ワークフロー強化 — gwt-spec 化、C# write contract/API/settings API 実装、Rust 202 tests pass、`dotnet test` 3 tests pass、Unity generated csproj build success、Unity batch EditMode `145 total / 139 passed / 0 failed / 6 skipped`
