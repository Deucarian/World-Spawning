using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Deucarian.WorldSpawning.Tests
{
    public sealed class WorldSpawnHostTests
    {
        [UnityTest]
        public IEnumerator DirectSpawnUsesPosePoolCapacityAndReleasesOwnedInstances()
        {
            var go = new GameObject("world");
            var prefab = new GameObject("enemy");
            try
            {
                var host = go.AddComponent<WorldSpawnHost>();
                host.Configure(new SpawnableCatalog(new[] { new SpawnableDefinition(new WorldSpawnableId("enemy.goblin"),
                    new GameObjectPrefabProvider(prefab), maximumCapacity: 1) }));
                var spawned = host.Spawn(new WorldSpawnHostTestsKey("enemy.goblin"), new Vector3(1, 2, 3));
                Assert.That(spawned.Succeeded, Is.True);
                Assert.That(spawned.Instance.transform.position, Is.EqualTo(new Vector3(1, 2, 3)));
                Assert.That(host.Spawn(new WorldSpawnHostTestsKey("enemy.goblin"), Vector3.zero).FailureReason, Is.EqualTo(SpawnFailureReason.CapacityExhausted));
                Assert.That(host.Despawn(spawned.InstanceId).Succeeded, Is.True);
                var next = host.Spawn(new WorldSpawnHostTestsKey("enemy.goblin"), Vector3.one);
                Assert.That(next.Succeeded, Is.True);
                Object.DestroyImmediate(go);
                yield return null;
                Assert.That(next.Instance == null, Is.True);
                Assert.That(prefab != null, Is.True);
            }
            finally { if (go != null) Object.DestroyImmediate(go); Object.DestroyImmediate(prefab); }
        }
    }
}
