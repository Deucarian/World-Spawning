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
    public sealed class SpawnChannelDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("position")] public Vector3 Position = Vector3.zero;
        [DefinitionField("eulerAngles")] public Vector3 EulerAngles = Vector3.zero;
    }
}
