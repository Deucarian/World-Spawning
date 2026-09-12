using System;
using System.Linq;
using UnityEngine;

namespace Deucarian.WorldSpawning.Unity
{
    public sealed class SpawnChannelDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/SpawnChannelDefinitionCatalog";
        [SerializeField] private SpawnChannelDefinitionAsset[] definitions = Array.Empty<SpawnChannelDefinitionAsset>();
        public static SpawnChannelDefinitionCatalog LoadProject() => Resources.Load<SpawnChannelDefinitionCatalog>(ResourcePath) ?? throw new InvalidOperationException("Create a SpawnChannel definition in Definitions before loading its project catalog.");
        public SpawnChannelDefinition[] CreateRuntimeDefinitions()
        {
            if (definitions.Any(x => x == null) || definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("The SpawnChannel catalog contains missing or duplicate definitions. Synchronize it in Definitions.");
            return definitions.Select(x => x.ToRuntimeDefinition()).ToArray();
        }
    }
}
