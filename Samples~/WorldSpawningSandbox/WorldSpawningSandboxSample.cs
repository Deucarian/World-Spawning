using System;
using System.Collections.Generic;
using Deucarian.Encounters;
using UnityEngine;

namespace Deucarian.WorldSpawning.Samples
{
    public static class WorldSpawningSandboxSample
    {
        public static void Run(GameObject prefab)
        {
            SpawnableId spawnable = new SpawnableId("enemy.sample");
            SpawnChannelId channel = new SpawnChannelId("lane.sample");
            var service = new WorldSpawnService(
                new SpawnableCatalog(new[] { new SpawnableDefinition(spawnable, new GameObjectPrefabProvider(prefab), 2, 8) }),
                new ChannelPoseResolver(new Dictionary<SpawnChannelId, SpawnPose> { [channel] = new SpawnPose(Vector3.zero, Quaternion.identity) }));

            service.Warmup();
            SpawnRequest request = new SpawnRequest(new EncounterId("encounter.sample"), new WaveId("wave.sample"), new SpawnGroupId("group.sample"), spawnable, channel, 0, 1, 0, 0);
            SpawnResult result = service.Spawn(request);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.FailureReason.ToString());
            }

            service.Despawn(result.InstanceId, DespawnReason.Requested);
            service.Dispose();
        }
    }
}
