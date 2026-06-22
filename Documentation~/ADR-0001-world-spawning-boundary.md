# ADR-0001: World Spawning Boundary

## Status

Accepted for 0.2.0.

## Decision

`com.deucarian.world-spawning` is a Unity-specific package that consumes generic `WorldSpawnRequest` values and turns them into runtime `GameObject` instances through explicit prefab providers, spawn-pose resolvers, and bounded pools.

It depends on `com.deucarian.gameplay-foundation`. It does not depend on Encounters, Combat, Progression, Persistence, UI packages, Core State, or Unity.Entities.

## Package Boundary

World Spawning owns prefab/provider mapping, spawn-channel to pose resolution, pooling, spawn/despawn lifecycle, active instance tracking, capacity limits, warmup, failure reporting, despawn reasons, optional reset hooks, parent/container organization, deterministic request consumption order, and diagnostics snapshots.

It does not own encounter scheduling, wave logic, combat damage, health, movement AI, tower placement, pathfinding, progression rewards, persistence, UI, rendering policy, or ECS.

## Separation From Encounters

Encounters remains pure C# and emits data-only `SpawnRequest` records. Application, Defense Games, or test adapters convert those records into generic `WorldSpawnRequest` values before calling World Spawning. This keeps encounter simulation testable without a scene while allowing Unity object lifecycle to evolve independently.

## Separation From Combat

Spawning creates and tracks objects; it does not decide whether they are alive, damaged, dead, valid targets, or reward-bearing. A combat adapter may despawn an instance with a reason, but Combat remains a separate package.

## Prefab And Provider Strategy

`SpawnableCatalog` maps `WorldSpawnableId` to `SpawnableDefinition`. A definition holds an `ISpawnPrefabProvider`, initial capacity, maximum capacity, and an optional pool root name. The default provider is `GameObjectPrefabProvider`, but tests and products can supply custom providers.

## Pooling Strategy

`GameObjectWorldSpawnPool` maintains one bucket per spawnable definition. Warmup instantiates inactive objects up to configured initial capacity. Spawn reuses inactive objects first, then creates new objects only until maximum capacity is reached. When capacity is exhausted, spawning returns a failure result and never silently drops the request.

Hot flow after warmup performs no direct `Instantiate` or `Destroy`. `Clear` may destroy pooled/active objects because it is a lifecycle cleanup operation, not a per-spawn path.

## Spawn Channel To Pose Resolution

Placement remains abstract:

`WorldSpawnChannelId -> ISpawnPoseResolver -> SpawnPose`

The package does not hard-code radial, lane, grid, path-node, perimeter, or center-core rules. Idle Auto Defense and classic Tower Defense provide resolvers through adapters or samples.

## Active Instance Identity

Each successful spawn receives a monotonically increasing `SpawnInstanceId`. Active records map IDs to spawned GameObjects, spawnable IDs, channels, and source request metadata. Duplicate instance IDs are rejected when registering active objects internally.

## Despawn Lifecycle

`Despawn` marks an active instance inactive, invokes optional object callbacks, resets transforms into the pool root, and returns the object to its bucket. Unknown or already despawned IDs return explicit `DespawnResult` values.

## Object Reset Policy

Objects may implement `IWorldSpawnedObject` for spawn/despawn notifications and `IWorldSpawnResettable` for pool reset. The pool invokes reset before reuse and on return so product-specific components can clear state without World Spawning knowing about combat, movement, or UI.

## Capacity And Backpressure

Capacity is per spawnable definition. Initial capacity warms the pool; maximum capacity bounds total active plus inactive instances. Exhaustion returns `SpawnFailureReason.CapacityExhausted`. No request is hidden or queued by the package.

## Failure Reporting

`SpawnResult` reports unknown spawnable IDs, missing/invalid prefab providers, pose resolution failures, invalid requests, capacity exhaustion, duplicate instance IDs, and pool failures. `DespawnResult` reports success, unknown instance, already despawned, and invalid input.

## Deterministic Request Order

`WorldSpawnService.SpawnMany` consumes request arrays in ascending request sequence order, preserving encounter determinism independently from caller buffer order.

## Unity Lifecycle And Scene Assumptions

The package requires an explicit owner/root transform or creates a named runtime root. It does not assume scene names, bootstrap objects, singleton services, `DontDestroyOnLoad`, or global pools.

## Future ECS Boundary

Future ECS integration should be a separate adapter that converts spawn requests to entity commands. This package may provide authoring lessons, but its GameObject pool and component callbacks are not the ECS runtime.

## Why Unity-Specific

Previous Phase 1 packages modeled pure gameplay state. World Spawning is intentionally Unity-specific because its responsibility is prefab, transform, active state, pooling, and scene-object lifecycle, which are not meaningful in pure C# alone.
