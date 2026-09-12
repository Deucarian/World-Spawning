using System;
using UnityEngine;

namespace Deucarian.WorldSpawning
{
    public sealed class SpawnChannelDefinition
    {
        public SpawnChannelDefinition(WorldSpawnChannelId id, Vector3 position, Vector3 eulerAngles)
        {
            if (id.IsEmpty) throw new ArgumentException("Choose an existing spawn channel.", nameof(id));
            if (!Finite(position) || !Finite(eulerAngles)) throw new ArgumentException("Spawn channel position and rotation must be finite.");
            Id = id; Position = position; Rotation = Quaternion.Euler(eulerAngles);
        }
        public WorldSpawnChannelId Id { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }
}
