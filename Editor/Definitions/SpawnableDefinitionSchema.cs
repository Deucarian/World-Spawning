using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    public sealed class SpawnableDefinitionSchema : DeucarianSerializedDefinitionSchema<SpawnableDefinitionAsset, SpawnableDefinitionSpec>
    {
        public override string Id => "spawnables";
        public override string DisplayName => "Spawnables";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((SpawnableDefinitionAsset)asset).ToRuntimeDefinition();
        }
        public override void RefreshCatalog(bool validateOnly = false) { ProjectDefinitionCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/World Spawning/Spawnable Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new SpawnableDefinitionSchema(), "NewSpawnable"); }
    }
}
