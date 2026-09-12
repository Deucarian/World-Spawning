using System;

using UnityEngine;

namespace Deucarian.WorldSpawning.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class SpawnChannelDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private Vector3 eulerAngles = Vector3.zero;
        public string Id => id;
        public string DisplayName => displayName;
        public SpawnChannelKey Key => new AssetKey(id);
        public SpawnChannelDefinition ToRuntimeDefinition() => new SpawnChannelDefinition(new WorldSpawnChannelId(Id), position, eulerAngles);
        private sealed class AssetKey : SpawnChannelKey { public AssetKey(string value) : base(value) { } }
    }
}
