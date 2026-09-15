using UnityEngine;

namespace Deucarian.WorldSpawning.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private SpawnableKey goblin = Spawnables.Goblin;

        [SerializeField] private WorldSpawnHost world;
        public void SpawnEnemy(Vector3 position) => world.Spawn(goblin, position);
    }
}
