# Deucarian World Spawning

`com.deucarian.world-spawning` is a Unity-facing adapter package that turns `Deucarian.Encounters.SpawnRequest` values into pooled runtime `GameObject` instances.

It owns prefab/provider mapping, spawn-channel pose resolution, pooling, lifecycle callbacks, active instance tracking, capacity limits, failure reporting, despawn reasons, and diagnostics snapshots.

Runtime dependencies:

- `com.deucarian.gameplay-foundation`
- `com.deucarian.encounters`

Out of scope:

- encounter scheduling, wave rules, combat, health, movement AI, tower placement, pathfinding, progression, persistence, UI, ECS, and product-specific idle or tower-defense rules.
