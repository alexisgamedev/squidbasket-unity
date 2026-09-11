# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

`squidbasket-unity` is a 3D arcade basketball shooter built in Unity 6 (URP). Core loop: aim/power
a shot from various spots on the court against a single hoop, score-attack style. Desktop-only
target, single-player, no monetization. The project is a fresh scaffold — gameplay (court/hoop
scene, ball physics, shot-arc/aim controller, scoring UI) has not been built yet.

- Editor version: **6000.0.58f2** (pin builds/opens to this version — see `ProjectSettings/ProjectVersion.txt`)
- Render pipeline: URP (`com.unity.render-pipelines.universal`)
- Source control: git, remote at `https://github.com/alexisgamedev/squidbasket-unity`

## Working with this project

This project is managed via the `unity` CLI rather than the Unity Hub GUI. Prefer it (or the
Unity MCP server, if configured) over hand-editing `Packages/manifest.json` or scene/asset YAML.

```bash
# Open the project in the Editor
unity open .

# Run the Editor for a specific version explicitly
unity 6000.0.58f2 .

# Build (see `unity build --help` for target list, e.g. StandaloneWindows64/StandaloneLinux64)
unity build . --editor-version 6000.0.58f2 --target StandaloneWindows64

# Run Play Mode / Edit Mode tests
unity test . --editor-version 6000.0.58f2 --mode EditMode
unity test . --editor-version 6000.0.58f2 --mode PlayMode

# Run a single test (filter by name/namespace)
unity test . --editor-version 6000.0.58f2 --mode PlayMode --filter "Namespace.ClassName.TestMethod"
```

### Adding/removing packages

**Never hand-edit `Packages/manifest.json`.** Packages are installed headlessly via the
`UnityEditor.PackageManager.Client` API, invoked through an Editor-only bootstrap script:

- [Assets/Editor/ProjectBootstrap/PackageInstaller.cs](Assets/Editor/ProjectBootstrap/PackageInstaller.cs) —
  edit the `PackagesToAdd`/`PackagesToRemove` arrays, then run it headless:
  ```bash
  "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe" -batchmode \
    -projectPath . -executeMethod ProjectBootstrap.PackageInstaller.Install -logFile -
  ```
  Must be invoked as a **direct Editor binary call without `-quit`** (not via `unity run`, which
  always injects `-quit` and would kill the Editor before the async UPM request resolves).
- [Assets/Editor/ProjectBootstrap/ProjectSaver.cs](Assets/Editor/ProjectBootstrap/ProjectSaver.cs) —
  forces an asset import/save pass headlessly (e.g. to generate `.meta` files after adding files
  outside the Editor). This one is synchronous and safe to run via `unity run`:
  ```bash
  unity run . --editor-version 6000.0.58f2 -- -executeMethod ProjectBootstrap.ProjectSaver.SaveAll
  ```

These two scripts are bootstrap conveniences, not gameplay code — safe to extend or delete.

### Committing

Every asset (`.cs`, `.unity`, `.asset`, `.mat`, `.fbx`, etc.) **must** be committed together with
its `.meta` file, or references break for the next person/Editor session. After adding files
outside the Editor, run the `ProjectSaver` pass above (or open the project once) before
committing so `.meta` files exist.

## Architecture / current state

- `Assets/Scenes/SampleScene.unity` — the only scene, still the stock template scene (Main
  Camera, Directional Light, Global Volume). No gameplay has been wired in yet.
- `Assets/Content/` — imported art: `Basketball Court.fbx` + texture/material. Not yet placed
  or wired into a scene.
- `Assets/Settings/` — URP pipeline assets (PC/Mobile renderer + render pipeline asset variants,
  volume profiles). Standard URP-template output; PC-quality settings are the active ones for
  this desktop-only build.
- `Assets/InputSystem_Actions.inputactions` — the default Input System action map from the
  template; not yet customized for shooting/aiming controls.
- `Assets/Editor/ProjectBootstrap/` — headless package-management bootstrap scripts (see above).

Key installed packages beyond the URP template defaults:
- `com.unity.cinemachine` — intended for shot-arc/follow camera framing (not yet wired into a scene).
- `com.unity.ai.navigation`, `com.unity.timeline` — template defaults, unused so far.

No custom `MonoBehaviour`/gameplay scripts exist yet outside the Editor bootstrap tooling — the
next work is the actual game (court/hoop layout, ball rigidbody + shot mechanic, aim/power
input, scoring/UI), which will introduce the real architecture (likely a `Assets/Scripts/`
runtime folder, not yet created).
