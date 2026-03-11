# LSP Write Safety

## Preview First

- Use `apply: false` for `rename_symbol`, `replace_symbol_body`, `insert_before_symbol`, `insert_after_symbol`, and `remove_symbol` when the scope is not trivial.
- Describe the intended edit in terms of symbol names before applying it.

## Symbol Targeting

- Use `get_symbols` to confirm the exact `namePath`.
- Keep `namePath` slash-separated from outer type to inner member.
- Use `find_refs` before destructive renames or removals.

## Validation

1. Apply the smallest viable edit.
2. Run `update_index` for touched paths.
3. Check `get_compilation_state`.
4. Report diagnostics before attempting unrelated follow-up changes.

## Class Creation

- Use `create_class` for new files instead of hand-writing boilerplate when possible.
- Use `validate_text_edits` when constructing larger manual replacements.
