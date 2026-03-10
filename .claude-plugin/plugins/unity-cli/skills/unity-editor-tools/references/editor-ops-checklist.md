# Editor Ops Checklist

## Safe Order

1. Ping the editor and capture `get_editor_state`.
2. Read the current state before changing it.
3. Apply one editor-wide change at a time.
4. Re-read the relevant state and summarize the delta.

## Settings Changes

- Use `get_project_settings` first so the response can mention the current value.
- Send the narrowest `update_project_settings` payload possible.
- Include `"confirmChanges": true` for mutating settings updates.

## Console and Profiler

- Read the console before clearing it.
- Check `profiler_status` before starting or stopping capture.
- Query `profiler_get_metrics --json '{"listAvailable":true}'` before requesting named metrics.

## Package Operations

- List installed packages before installing or removing one.
- Treat registry changes as project-wide mutations and call them out explicitly.
- If package changes are part of a larger workflow, mention any restart or reimport implications.
