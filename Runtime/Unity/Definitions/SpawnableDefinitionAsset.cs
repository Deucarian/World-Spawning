using System;

using UnityEngine;

namespace Deucarian.WorldSpawning.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class SpawnableDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private GameObject prefab = null;
        [SerializeField] private int initialCapacity = 0;
        [SerializeField] private int maximumCapacity = 64;
        public string Id => id;
        public string DisplayName => displayName;
        public SpawnableKey Key => new AssetKey(id);
        public SpawnableDefinition ToRuntimeDefinition()
        {
            if (prefab == null) throw new InvalidOperationException("Assign a prefab to spawnable '" + DisplayName + "' in Definitions.");
            return new SpawnableDefinition(new WorldSpawnableId(Id), new GameObjectPrefabProvider(prefab), initialCapacity, maximumCapacity);
        }
        private sealed class AssetKey : SpawnableKey { public AssetKey(string value) : base(value) { } }
    }
}
