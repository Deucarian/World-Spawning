# API

Namespace: `Deucarian.WorldSpawning`

## Core Types

- `WorldSpawnService`: consumes `SpawnRequest` values and creates/despawns pooled GameObjects.
- `SpawnableCatalog`: validates and stores `SpawnableDefinition` entries.
- `SpawnableDefinition`: maps a `SpawnableId` to an `ISpawnPrefabProvider`, initial capacity, max capacity, and pool root name.
- `SpawnInstanceId`: monotonically increasing active instance identifier.
- `SpawnPose`: Unity position, rotation, and optional parent.
- `ISpawnPoseResolver`: maps `SpawnChannelId` from a request to a `SpawnPose`.
- `ISpawnPrefabProvider`: returns a prefab for a spawnable.
- `GameObjectPrefabProvider`: fixed prefab provider.
- `IWorldSpawnPool`: pooling abstraction.
- `GameObjectWorldSpawnPool`: default GameObject pool.
- `IWorldSpawnedObject`: optional spawn/despawn notification contract.
- `IWorldSpawnResettable`: optional reset contract for pooled objects.
- `SpawnResult`, `DespawnResult`
- `WorldSpawnSnapshot`, `SpawnPoolSnapshot`, `ActiveSpawnSnapshot`
- `SpawnFailureReason`, `DespawnReason`, `DespawnFailureReason`

## Main Flow

1. Build a `SpawnableCatalog`.
2. Provide an `ISpawnPoseResolver`.
3. Construct `WorldSpawnService`.
4. Call `Warmup`.
5. Drain `SpawnRequest` values from Encounters.
6. Call `Spawn` or `SpawnMany`.
7. Call `Despawn` when an external system decides the object lifecycle is complete.

`SpawnMany` sorts the supplied request slice by sequence before spawning.

## Runtime Dependencies

- `com.deucarian.gameplay-foundation`
- `com.deucarian.encounters`

No runtime dependency on Combat, Progression, Persistence, UI packages, Core State, or Unity.Entities.
