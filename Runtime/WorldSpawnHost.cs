using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.WorldSpawning
{
    /// <summary>Owns configured prefab pools and explicit-position spawning for one world.</summary>
    [DisallowMultipleComponent]
    public sealed class WorldSpawnHost : MonoBehaviour
    {
        [Serializable]
        private sealed class Entry
        {
            public SpawnableKey key;
            public GameObject prefab;
            [Min(0)] public int initialCapacity;
            [Min(1)] public int maximumCapacity = 64;
        }
        [SerializeField] private Entry[] spawnables = Array.Empty<Entry>();
        [SerializeField] private Unity.SpawnableDefinitionCatalog definitionCatalog;
        [SerializeField] private Unity.SpawnChannelDefinitionCatalog channelCatalog;
        private Dictionary<string, SpawnChannelDefinition> channels;
        private readonly DirectPoses poses = new DirectPoses();
        private WorldSpawnService service;
        private long sequence;
        private bool destroyed;
        public int ActiveCount => service?.ActiveCount ?? 0;

        public void Configure(SpawnableCatalog catalog)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(WorldSpawnHost));
            if (service != null) throw new InvalidOperationException("The spawn host is already configured.");
            service = new WorldSpawnService(catalog, poses, rootName: name + " Pools");
        }

        public SpawnResult Spawn(SpawnableKey key, Vector3 position) => Spawn(key, position, Quaternion.identity);
        public SpawnResult Spawn(SpawnableKey key, SpawnChannelKey channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel), "Select an existing spawn channel.");
            if (channels == null)
            {
                channels = new Dictionary<string, SpawnChannelDefinition>(StringComparer.Ordinal);
                foreach (var definition in (channelCatalog != null ? channelCatalog : Unity.SpawnChannelDefinitionCatalog.LoadProject()).CreateRuntimeDefinitions()) channels.Add(definition.Id.Value, definition);
            }
            if (!channels.TryGetValue(channel.Id, out var selected)) throw new InvalidOperationException("Spawn channel '" + channel.Id + "' is missing. Synchronize the channel definitions.");
            return Spawn(key, transform.TransformPoint(selected.Position), transform.rotation * selected.Rotation);
        }
        public SpawnResult Spawn(SpawnableKey key, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key), "Select a SpawnableKey or pass a named spawnable definition.");
            EnsureConfigured();
            var request = new WorldSpawnRequest(new WorldSpawnableId(key.Id), new WorldSpawnChannelId("direct"), ++sequence);
            poses.Values.Add(request.Sequence, new SpawnPose(position, rotation, parent));
            try { return service.Spawn(request); }
            finally { poses.Values.Remove(request.Sequence); }
        }
        public DespawnResult Despawn(SpawnInstanceId id, DespawnReason reason = DespawnReason.Requested)
        { EnsureConfigured(); return service.Despawn(id, reason); }
        public void Warmup() { EnsureConfigured(); service.Warmup(); }
        private void EnsureConfigured()
        {
            if (destroyed) throw new ObjectDisposedException(nameof(WorldSpawnHost));
            if (service != null) return;
            if (definitionCatalog != null || spawnables.Length == 0)
            {
                var catalog = definitionCatalog != null ? definitionCatalog : Resources.Load<Unity.SpawnableDefinitionCatalog>(Unity.SpawnableDefinitionCatalog.ResourcePath);
                if (catalog != null) { Configure(new SpawnableCatalog(catalog.CreateRuntimeDefinitions())); return; }
            }
            var definitions = new List<SpawnableDefinition>();
            foreach (var entry in spawnables)
            {
                if (entry == null || entry.key == null || entry.prefab == null) throw new InvalidOperationException("WorldSpawnHost '" + name + "' has an incomplete spawn entry. Select its SpawnableKey and assign its prefab in the Inspector.");
                definitions.Add(new SpawnableDefinition(new WorldSpawnableId(entry.key.Id),
                    new GameObjectPrefabProvider(entry.prefab), entry.initialCapacity, entry.maximumCapacity));
            }
            Configure(new SpawnableCatalog(definitions));
        }
        private void OnDestroy() { destroyed = true; service?.Dispose(); service = null; poses.Values.Clear(); }
        private sealed class DirectPoses : ISpawnPoseResolver
        {
            public readonly Dictionary<long, SpawnPose> Values = new Dictionary<long, SpawnPose>();
            public SpawnPoseResult TryResolvePose(WorldSpawnRequest request) => Values.TryGetValue(request.Sequence, out var pose)
                ? SpawnPoseResult.Success(pose) : SpawnPoseResult.Failure("No pose was supplied for this request.");
        }
    }
}
