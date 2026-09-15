using System;

namespace Deucarian.WorldSpawning
{
    /// <summary>Marks an authoritative set of named SpawnChannelKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SpawnChannelKeySetAttribute : Attribute { }
}
