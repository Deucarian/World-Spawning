# Deucarian World Spawning

`com.deucarian.world-spawning` is a Unity-facing package that turns generic `WorldSpawnRequest` values into pooled runtime `GameObject` instances.

As of `0.2.0`, Encounters is one possible source of spawn requests, not a runtime dependency. Application or test adapters convert Encounter `SpawnRequest` values into `WorldSpawnRequest` values.

It owns prefab/provider mapping, spawn-channel pose resolution, pooling, lifecycle callbacks, active instance tracking, capacity limits, failure reporting, despawn reasons, and diagnostics snapshots.

Runtime dependencies:

- `com.deucarian.gameplay-foundation`

Out of scope:

- encounter scheduling, wave rules, combat, health, movement AI, tower placement, pathfinding, progression, persistence, UI, ECS, and product-specific idle or tower-defense rules.

## Install

Stable:

```json
"com.deucarian.world-spawning": "https://github.com/Deucarian/World-Spawning.git#main"
```

Development:

```json
"com.deucarian.world-spawning": "https://github.com/Deucarian/World-Spawning.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## When To Use This

Use this package when you need Generic Unity GameObject spawning for world spawn requests, prefab providers, pose resolvers, pooling, lifecycle, and diagnostics snapshots.

Do not use this package to take ownership of capabilities outside its `AGENTS.md` boundary. Reusable behavior should stay with the package that owns that capability in the Package Registry governance docs.

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Import the `World Spawning Sandbox` sample if you want a working reference scene or setup.
4. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Validation

Run the shared package validator from this repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.

## License

MIT. See `LICENSE.md`.
