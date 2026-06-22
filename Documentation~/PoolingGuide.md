# Pooling Guide

Each `SpawnableDefinition` has `InitialCapacity` and `MaximumCapacity`.

- `InitialCapacity` is created during `WorldSpawnService.Warmup`.
- `MaximumCapacity` is the total active plus pooled object cap.
- Spawn first reuses inactive objects.
- Spawn creates cold instances only when below maximum capacity.
- If the pool is exhausted, spawn returns `SpawnFailureReason.CapacityExhausted`.
- Despawn returns the object to its bucket and invokes reset/despawn callbacks.
- `Clear` may destroy objects because it is a lifecycle cleanup operation.

After warmup, a spawn/despawn loop within warmed capacity does not instantiate or destroy objects. Editor allocation measurements are recorded in package validation notes; they are not mobile or IL2CPP claims.
