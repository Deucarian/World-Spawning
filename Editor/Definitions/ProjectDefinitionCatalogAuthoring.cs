using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    internal static class ProjectDefinitionCatalogAuthoring
    {
        internal static void Refresh(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:SpawnableDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<SpawnableDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("Spawnable definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<SpawnableDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/SpawnableDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
    }
}
