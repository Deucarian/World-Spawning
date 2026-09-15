using System;
using UnityEngine;

namespace Deucarian.WorldSpawning.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class WorldSpawningWorkflow : MonoBehaviour
    {
        [SerializeField] private SpawnTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public void Spawn() { trigger.Spawn(); status = "One pooled instance is active at the selected channel."; }
        public void Despawn() { trigger.Despawn(); status = "Returned the instance to its pool."; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("World-Spawning — definition workflow");
            GUILayout.Label("SpawnTrigger selects a spawnable and a channel from typed dropdowns. WorldSpawnHost owns the pool and cleanup.");
            GUILayout.Space(12);
            if (GUILayout.Button("Spawn / replace", GUILayout.Height(32))) { try { Spawn(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Despawn", GUILayout.Height(32))) { try { Despawn(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
