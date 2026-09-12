using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.WorldSpawning.Editor
{
    [CustomPropertyDrawer(typeof(SpawnChannelKey), true)]
    public sealed class SpawnChannelKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(SpawnChannelKey);
        public override Type DefinitionSetAttribute => typeof(SpawnChannelKeySetAttribute);
        public override string SetupHint => "Select an existing SpawnChannelKey; create reusable channels in Definitions.";
    }
}
