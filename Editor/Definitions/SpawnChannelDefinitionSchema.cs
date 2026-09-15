using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    public sealed class SpawnChannelDefinitionSchema : DeucarianSerializedDefinitionSchema<SpawnChannelDefinitionAsset, SpawnChannelDefinitionSpec>
    {
        public override string Id => "spawn-channels";
        public override string DisplayName => "Spawn channels";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((SpawnChannelDefinitionAsset)asset).ToRuntimeDefinition();
        }
        public override void RefreshCatalog(bool validateOnly = false) { SpawnChannelCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/World Spawning/SpawnChannel Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new SpawnChannelDefinitionSchema(), "NewSpawnChannel"); }
    }
}
