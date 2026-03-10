# tasks/lessons.md

このファイルは、ユーザーからの修正や失敗から得た再発防止ルールを記録する。

## Rules

- ルールは具体的に書く（「何を」「いつ」「どう防ぐか」）
- 再発した場合はルールを更新して、曖昧な文言を削る
- 直近の作業開始前に必ず読み返す

## Entries

### 2026-02-27

- Context: Markdown Lint を `npm run lint:md` で実行した際、ローカルに `markdownlint` が存在しなかった。
- Mistake: コマンド失敗後の代替手順を標準化していなかった。
- Rule: ツールが未インストールの環境では `npx --yes <tool>` で即時フォールバックする。
- Checkpoint: `npx --yes markdownlint-cli CLAUDE.md tasks/*.md --config .markdownlint.json --ignore-path .markdownlintignore`

### 2026-03-10

- Context: `gwt-spec` ラベル付き Issue #107 の実装後、Issue 本文の `Tasks` が未更新のまま「残り 2 点」と判断してしまった。
- Mistake: 完了判定の一次情報を Issue 本文ではなく、自分の縮約した内部チェックリストに置いてしまった。
- Rule: `gwt-spec` Issue の完了判定は必ず Issue 本文の `Tasks` / 受け入れ基準 / PR本文 / 作業ツリー状態を同期させた上で行う。1つでも未同期なら「完了」と言わない。
- Checkpoint: 1. Issue 本文の `Tasks` を更新 2. 検証結果を Issue/PR に反映 3. `git status --short` を確認 4. ignore すべきローカル生成物が残っていれば `.gitignore` か cleanup を先に行う
