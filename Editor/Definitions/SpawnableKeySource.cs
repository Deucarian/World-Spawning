using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    public sealed class SpawnableKeySource : DeucarianAssetKeySource<SpawnableDefinitionAsset>
    {
        public override Type KeyType => typeof(SpawnableKey);
        public override Type DefinitionSetAttribute => typeof(SpawnableKeySetAttribute);
        public override string GeneratedClassName => "ProjectSpawnables";
        protected override DeucarianKeyChoice ReadDefinition(SpawnableDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
