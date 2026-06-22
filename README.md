# Deucarian World Spawning

`com.deucarian.world-spawning` is a Unity-facing package that turns generic `WorldSpawnRequest` values into pooled runtime `GameObject` instances.

As of `0.2.0`, Encounters is one possible source of spawn requests, not a runtime dependency. Application or test adapters convert Encounter `SpawnRequest` values into `WorldSpawnRequest` values.

It owns prefab/provider mapping, spawn-channel pose resolution, pooling, lifecycle callbacks, active instance tracking, capacity limits, failure reporting, despawn reasons, and diagnostics snapshots.

Runtime dependencies:

- `com.deucarian.gameplay-foundation`

Out of scope:

- encounter scheduling, wave rules, combat, health, movement AI, tower placement, pathfinding, progression, persistence, UI, ECS, and product-specific idle or tower-defense rules.
