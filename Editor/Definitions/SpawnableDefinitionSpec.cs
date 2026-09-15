using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.WorldSpawning.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WorldSpawning.Editor.Definitions
{
    [Serializable]
    public sealed class SpawnableDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("prefab")] public GameObject Prefab = null;
        [DefinitionField("initialCapacity")] public int InitialCapacity = 0;
        [DefinitionField("maximumCapacity")] public int MaximumCapacity = 64;
    }
}
