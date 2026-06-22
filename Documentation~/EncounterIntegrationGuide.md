# Encounter Integration Guide

Encounters emits `SpawnRequest` values with `SpawnableId`, `SpawnChannelId`, sequence, wave, group, and tick metadata.

World Spawning consumes those requests:

```csharp
EncounterDrainResult drained = encounter.DrainSpawnRequests(requestBuffer);
worldSpawnService.SpawnMany(requestBuffer, drained.Written, resultBuffer);
```

Encounters does not know about prefabs, transforms, scene roots, positions, pooling, or active GameObjects. External systems may update active-count metrics manually from `WorldSpawnService.ActiveCount` or `WorldSpawnSnapshot`.
