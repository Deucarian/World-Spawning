# Deucarian World Spawning Agent Notes

Package ID: `com.deucarian.world-spawning`
Repository: `Deucarian/World-Spawning`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Unity-facing world spawn requests, spawn channels, pose resolution, prefab providers, GameObject pooling, active instance tracking, capacity/failure reporting, despawn reasons, lifecycle callbacks, and spawn diagnostics snapshots.

Registered capabilities:
- None.

This package must not own:

- Encounter scheduling, wave rules, combat/health, attack selection, projectile behavior, weapon orchestration, pathfinding, movement AI, tower placement, progression, persistence, UI, ECS/DOTS, or product-specific defense-game rules.

## Dependencies

Allowed dependency shape:

- May depend on Gameplay Foundation for gameplay IDs and deterministic shared primitives.
- May depend on Common for approved transient Unity object cleanup.

Required dependencies and why:

- `com.deucarian.common`: approved `UnityObjectUtility.DestroySafely` cleanup for pooled GameObject instances and runtime-owned roots.
- `com.deucarian.gameplay-foundation`: content IDs, deterministic identifiers, and shared gameplay primitives used by spawn requests and diagnostics.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- None.

## Policies

- Keep this package focused on generic world spawning and pooling.
- Do not add hard dependencies on Encounters, Combat, World Navigation, Attacks, Weapon Systems, Progression, Persistence, UI, or template/framework packages.
- Convert external package concepts into `WorldSpawnRequest` values in adapters owned by the caller or integration package.
- Logging: Do not introduce direct Unity Debug calls.
- Unity object lifetime: Use Common's `UnityObjectUtility.DestroySafely` for production cleanup.
- Testing: Test fixture teardown may use Unity `DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback, and Bootstrap fallback together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.
