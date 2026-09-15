using System;
using System.Linq;
using UnityEngine;

namespace Deucarian.WorldSpawning.Unity
{
    public sealed class SpawnableDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/SpawnableDefinitionCatalog";
        [SerializeField] private SpawnableDefinitionAsset[] definitions = Array.Empty<SpawnableDefinitionAsset>();
        public static SpawnableDefinitionCatalog LoadProject() => Resources.Load<SpawnableDefinitionCatalog>(ResourcePath) ?? throw new InvalidOperationException("Create a Spawnable definition in Definitions before loading its project catalog.");
        public SpawnableDefinition[] CreateRuntimeDefinitions()
        {
            if (definitions.Any(x => x == null) || definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("The Spawnable catalog contains missing or duplicate definitions. Synchronize it in Definitions.");
            return definitions.Select(x => x.ToRuntimeDefinition()).ToArray();
        }
    }
}
