# LSP Write Safety

## Preview First

- Use `apply: false` for `rename_symbol`, `replace_symbol_body`, `insert_before_symbol`, `insert_after_symbol`, and `remove_symbol` when the scope is not trivial.
- Describe the intended edit in terms of symbol names before applying it.

## Result Contract

- Treat `success`, `applied`, `changedFiles`, `changedSymbols`, `diagnostics`, `diffPreview`, `reason`, and `backend` as the canonical write result.
- `success: true` with `applied: false` usually means preview mode or a no-op. Inspect `reason` and `diffPreview` before assuming the change landed.
- Use `changedFiles` and `changedSymbols` to confirm the actual scope of a write, especially after multi-file or fallback-heavy operations.

## Symbol Targeting

- Use `get_symbols` to confirm the exact `namePath`.
- Keep `namePath` slash-separated from outer type to inner member.
- Use `find_refs` before destructive renames or removals.
- If `namePath` is unstable or incomplete, switch to `write_csharp_file` or `apply_csharp_edits`.

## Validation

1. Apply the smallest viable edit.
2. Enable `refresh`, `waitForCompile`, and `updateIndex` when later steps depend on fresh editor state, diagnostics, or symbol search.
3. Run `get_compilation_state` for non-trivial edits or when diagnostics are still unclear.
4. Report and fix real diagnostics before attempting unrelated follow-up changes.

## Class Creation

- Use `create_class` for new files instead of hand-writing boilerplate when possible.
- Use `validate_text_edits` when constructing larger manual replacements.
