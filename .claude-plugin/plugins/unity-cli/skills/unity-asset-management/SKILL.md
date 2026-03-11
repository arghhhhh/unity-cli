---
name: unity-asset-management
description: Manage Unity assets and import metadata with unity-cli. Use when the user asks to refresh the Asset Database, inspect asset info, create or modify materials, update import settings, or analyze asset dependencies before moving or deleting files. Do not use for Addressables-specific workflows or scene object edits.
allowed-tools: Bash, Read, Grep, Glob
metadata:
  author: akiojin
  version: 0.2.0
  category: assets
---

# Asset & Material Management

Manage the Unity Asset Database, create or modify materials, and control import settings.
Read `references/asset-safety.md` when the task involves dependency analysis, import changes, or material updates that may affect many assets.

## Use When

- The user wants to inspect, refresh, move, or otherwise manage project assets.
- The user wants to create or update materials.
- The user needs import settings or dependency analysis before file changes.

## Do Not Use When

- The task is specifically about Addressables groups and content builds.
- The request is about scene object or prefab editing rather than asset files.

## Recommended Flow

1. Inspect the target asset or material before changing it.
2. Run dependency analysis before deleting, moving, or changing shared assets.
3. Apply import or material changes with the narrowest possible payload.
4. Refresh the Asset Database if files changed outside the editor.

## Commands

```bash
# Asset database
unity-cli raw manage_asset_database --json '{"action":"refresh"}'
unity-cli raw manage_asset_database --json '{"action":"get_asset_info","assetPath":"Assets/Textures/hero.png"}'
unity-cli raw refresh_assets --json '{}'

# Materials
unity-cli raw create_material --json '{"materialPath":"Assets/Materials/HeroMat.mat","shader":"Standard"}'
unity-cli raw modify_material --json '{"materialPath":"Assets/Materials/HeroMat.mat","properties":{"_Color":{"r":1,"g":0,"b":0,"a":1}}}'

# Import settings
unity-cli raw manage_asset_import_settings --json '{"action":"modify","assetPath":"Assets/Textures/hero.png","settings":{"maxTextureSize":1024}}'

# Dependency analysis
unity-cli raw analyze_asset_dependencies --json '{"action":"get_dependencies","assetPath":"Assets/Prefabs/Player.prefab","recursive":true}'
```

## Examples

- "Refresh the asset database and inspect `Assets/Textures/hero.png`."
- "Create a material for the player and tint it red."
- "Check which assets depend on `Player.prefab` before I move it."

## Common Issues

- Asset changes made outside Unity are not visible: run `refresh_assets`.
- Import changes affect more files than expected: inspect the asset first and keep the payload focused.
- Deleting or moving assets is risky: use `analyze_asset_dependencies` before the mutation.
- Addressables content issues belong in `unity-addressables`, not this skill.
