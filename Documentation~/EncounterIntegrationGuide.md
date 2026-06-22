# Encounter Integration Guide

Encounters emits `SpawnRequest` values with Encounter-owned `SpawnableId`, `SpawnChannelId`, sequence, wave, group, and scheduled tick metadata. World Spawning consumes `WorldSpawnRequest`, so conversion happens at the composition boundary.

```csharp
WorldSpawnRequest worldRequest = new WorldSpawnRequest(
    new WorldSpawnableId(encounterRequest.SpawnableId.Value),
    new WorldSpawnChannelId(encounterRequest.ChannelId.Value),
    encounterRequest.Sequence,
    new WorldSpawnRequestContext(
        "encounters",
        encounterRequest.EncounterId.Value,
        encounterRequest.WaveId.Value,
        encounterRequest.GroupId.Value,
        0,
        (int)encounterRequest.ScheduledTick));
```

World Spawning consumes those requests:

```csharp
EncounterDrainResult drained = encounter.DrainSpawnRequests(requestBuffer);
worldSpawnService.SpawnMany(requestBuffer, drained.Written, resultBuffer);
```

Encounters does not know about prefabs, transforms, scene roots, positions, pooling, or active GameObjects. External systems may update active-count metrics manually from `WorldSpawnService.ActiveCount` or `WorldSpawnSnapshot`.
