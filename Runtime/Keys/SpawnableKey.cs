using System;
using UnityEngine;

namespace Deucarian.WorldSpawning
{
    /// <summary>A declared Spawnable identity. Reuse a named definition or select it in the Inspector.</summary>
    [Serializable]
    public class SpawnableKey : IEquatable<SpawnableKey>
    {
        [SerializeField] private string definitionId;

        /// <summary>For central definition sets and generated declarations; ordinary callers reuse those keys.</summary>
        protected SpawnableKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                throw new ArgumentException("A SpawnableKey definition needs a non-empty stable ID without surrounding whitespace.", nameof(id));
            definitionId = id;
        }

        public string Id => !string.IsNullOrWhiteSpace(definitionId) ? definitionId :
            throw new InvalidOperationException("No SpawnableKey is selected. Select an existing definition in the Inspector or assign a named key from a SpawnableKeySet declaration.");
        public bool Equals(SpawnableKey other) => other != null && string.Equals(definitionId, other.definitionId, StringComparison.Ordinal);
        public override bool Equals(object other) => other is SpawnableKey key && Equals(key);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(definitionId ?? string.Empty);
        public override string ToString() => definitionId ?? string.Empty;
    }
}
