using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.WorldSpawning.Editor
{
    [CustomPropertyDrawer(typeof(SpawnableKey), true)]
    public sealed class SpawnableKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(SpawnableKey);
        public override Type DefinitionSetAttribute => typeof(SpawnableKeySetAttribute);
        public override string SetupHint => "Select an existing SpawnableKey; declare reusable keys once in a [SpawnableKeySet] class.";
    }
}
