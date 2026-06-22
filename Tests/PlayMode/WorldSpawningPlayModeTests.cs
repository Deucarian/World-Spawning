using System.Collections;
using Deucarian.Encounters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Deucarian.WorldSpawning.Tests
{
    public sealed class WorldSpawningPlayModeTests
    {
        [UnityTest]
        public IEnumerator SpawnedObject_IsActiveForAFrame_ThenDespawns()
        {
            GameObject prefab = new GameObject("playmode-prefab");
            WorldSpawnService service = new WorldSpawnService(
                new SpawnableCatalog(new[] { new SpawnableDefinition(new SpawnableId("enemy.playmode"), new GameObjectPrefabProvider(prefab), 1, 1) }),
                new ChannelPoseResolver(new System.Collections.Generic.Dictionary<SpawnChannelId, SpawnPose>
                {
                    [new SpawnChannelId("channel.playmode")] = new SpawnPose(new Vector3(1, 2, 3), Quaternion.identity)
                }));
            service.Warmup();
            SpawnResult result = service.Spawn(new SpawnRequest(new EncounterId("encounter.playmode"), new WaveId("wave.playmode"), new SpawnGroupId("group.playmode"), new SpawnableId("enemy.playmode"), new SpawnChannelId("channel.playmode"), 0, 1, 0, 0));
            Assert.IsTrue(result.Succeeded);
            yield return null;
            Assert.IsTrue(result.Instance.activeSelf);
            Assert.AreEqual(new Vector3(1, 2, 3), result.Instance.transform.position);
            Assert.IsTrue(service.Despawn(result.InstanceId, DespawnReason.Requested).Succeeded);
            service.Dispose();
            Object.Destroy(prefab);
        }
    }
}
