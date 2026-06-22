# Changelog

## 0.2.0 - 2026-06-22

- Added generic `WorldSpawnRequest`, `WorldSpawnableId`, `WorldSpawnChannelId`, and `WorldSpawnRequestContext`.
- Removed the runtime dependency on `com.deucarian.encounters`.
- Refactored service, pose resolver, prefab provider, snapshots, and deterministic request ordering to use generic world spawn requests.
- Added Encounters adapter proof coverage in tests.

## 0.1.0 - 2026-06-22

- Added Unity GameObject world spawning adapter for Encounters spawn requests.
- Added spawnable catalogs, prefab providers, pose resolvers, bounded pooling, active instance tracking, lifecycle callbacks, snapshots, tests, benchmarks, docs, and samples.
