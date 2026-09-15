using System;
using UnityEngine;

namespace Deucarian.WorldSpawning
{
    /// <summary>One replaceable spawned instance. The WorldSpawnHost owns its pool and lifetime.</summary>
    [AddComponentMenu("Deucarian/World Spawning/Spawn Trigger")]
    public sealed class SpawnTrigger : MonoBehaviour
    {
        [SerializeField] private WorldSpawnHost host;
        [SerializeField] private SpawnableKey spawnable;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private SpawnChannelKey channel;
        private SpawnInstanceId instance;
        public SpawnResult SpawnInstance()
        {
            if (host == null) throw new InvalidOperationException("Assign a WorldSpawnHost to SpawnTrigger '" + name + "'.");
            Despawn();
            var point = spawnPoint != null ? spawnPoint : transform;
            var result = channel != null ? host.Spawn(spawnable, channel) : host.Spawn(spawnable, point.position, point.rotation);
            if (result.Succeeded) instance = result.InstanceId;
            return result;
        }
        public void Spawn()
        {
            var result = SpawnInstance();
            if (!result.Succeeded) throw new InvalidOperationException("SpawnTrigger '" + name + "' failed: " + result.Message + " Check the definition prefab and pool capacity.");
        }
        public void Despawn()
        {
            if (host != null && instance.Value > 0) host.Despawn(instance);
            instance = default;
        }
    }
}
