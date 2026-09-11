# Simple usage

Add WorldSpawnHost and configure its spawnable IDs, prefabs, initial capacities, and maximum capacities in the Inspector. Alternatively pass a SpawnableCatalog to Configure once. Spawn returns the existing SpawnResult, including InstanceId for world.Despawn(result.InstanceId). Position, rotation, and optional parent are supplied per call. The host owns its private pool root and releases all owned instances on destruction. Prefabs and supplied parents remain caller-owned. Warmup can be called once when preallocation is desired.

Import the **Simple Usage** sample from Unity Package Manager. Its caller script is:

```csharp
using UnityEngine;

namespace Deucarian.WorldSpawning.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private WorldSpawnHost world;
        public void SpawnEnemy(Vector3 position) => world.Spawn("enemy.goblin", position);
    }
}
```
