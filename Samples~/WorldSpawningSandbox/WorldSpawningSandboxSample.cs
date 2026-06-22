using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.WorldSpawning.Samples
{
    public static class WorldSpawningSandboxSample
    {
        public static void Run(GameObject prefab)
        {
            WorldSpawnableId spawnable = new WorldSpawnableId("enemy.sample");
            WorldSpawnChannelId channel = new WorldSpawnChannelId("lane.sample");
            var service = new WorldSpawnService(
                new SpawnableCatalog(new[] { new SpawnableDefinition(spawnable, new GameObjectPrefabProvider(prefab), 2, 8) }),
                new ChannelPoseResolver(new Dictionary<WorldSpawnChannelId, SpawnPose> { [channel] = new SpawnPose(Vector3.zero, Quaternion.identity) }));

            service.Warmup();
            WorldSpawnRequest request = new WorldSpawnRequest(spawnable, channel, 1, new WorldSpawnRequestContext("sample"));
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
