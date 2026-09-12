using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    internal static class SpawnChannelCatalogAuthoring
    {
        internal static void Refresh(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:SpawnChannelDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<SpawnChannelDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("SpawnChannel definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<SpawnChannelDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/SpawnChannelDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
    }
}
