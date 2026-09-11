using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    public sealed class SpawnChannelKeySource : DeucarianAssetKeySource<SpawnChannelDefinitionAsset>
    {
        public override Type KeyType => typeof(SpawnChannelKey);
        public override Type DefinitionSetAttribute => typeof(SpawnChannelKeySetAttribute);
        public override string GeneratedClassName => "ProjectSpawnChannels";
        protected override DeucarianKeyChoice ReadDefinition(SpawnChannelDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
