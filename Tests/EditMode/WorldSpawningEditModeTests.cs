using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Deucarian.Encounters;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.WorldSpawning.Tests
{
    public sealed class WorldSpawningEditModeTests
    {
        private static readonly EncounterId Encounter = new EncounterId("encounter.test");
        private static readonly WaveId Wave = new WaveId("wave.test");
        private static readonly SpawnGroupId Group = new SpawnGroupId("group.test");
        private static readonly SpawnChannelId ChannelA = new SpawnChannelId("channel.a");
        private static readonly SpawnChannelId ChannelB = new SpawnChannelId("channel.b");
        private static readonly SpawnableId EnemyA = new SpawnableId("enemy.a");
        private static readonly SpawnableId EnemyB = new SpawnableId("enemy.b");

        [Test]
        public void CatalogValidation_RejectsInvalidDefinitions()
        {
            GameObject prefab = Prefab("catalog-prefab");
            try
            {
                Assert.Throws<ArgumentException>(() => new SpawnableCatalog(new[] { Def(EnemyA, prefab), Def(EnemyA, prefab) }));
                Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnableDefinition(EnemyA, new GameObjectPrefabProvider(prefab), 2, 1));
            }
            finally { UnityEngine.Object.DestroyImmediate(prefab); }
        }

        [Test]
        public void SpawnFailures_ReportUnknownInvalidProviderPoseAndCapacity()
        {
            GameObject prefab = Prefab("failure-prefab");
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefab, 0, 1) });
            try
            {
                Assert.AreEqual(SpawnFailureReason.UnknownSpawnable, service.Spawn(Request(EnemyB)).FailureReason);
                Assert.AreEqual(SpawnFailureReason.InvalidRequest, service.Spawn(default).FailureReason);
                Assert.AreEqual(SpawnFailureReason.PoseResolutionFailed, service.Spawn(Request(EnemyA, new SpawnChannelId("channel.missing"))).FailureReason);
                SpawnResult first = service.Spawn(Request(EnemyA));
                Assert.IsTrue(first.Succeeded);
                Assert.AreEqual(SpawnFailureReason.CapacityExhausted, service.Spawn(Request(EnemyA, ChannelA, 2)).FailureReason);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefab); }

            WorldSpawnService missingProvider = Service(new[] { new SpawnableDefinition(EnemyA, null, 0, 1) });
            try { Assert.AreEqual(SpawnFailureReason.MissingPrefabProvider, missingProvider.Spawn(Request(EnemyA)).FailureReason); }
            finally { missingProvider.Dispose(); }

            WorldSpawnService invalidPrefab = Service(new[] { new SpawnableDefinition(EnemyA, new GameObjectPrefabProvider(null), 0, 1) });
            try { Assert.AreEqual(SpawnFailureReason.InvalidPrefab, invalidPrefab.Spawn(Request(EnemyA)).FailureReason); }
            finally { invalidPrefab.Dispose(); }
        }

        [Test]
        public void PoolWarmupSpawnDespawnReuseAndReset_Work()
        {
            GameObject prefab = Prefab("pooled-prefab");
            prefab.AddComponent<ProbeSpawnedObject>();
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefab, 1, 2) });
            try
            {
                service.Warmup();
                Assert.AreEqual(1, service.CreateSnapshot().Pools[0].PooledCount);
                SpawnResult first = service.Spawn(Request(EnemyA));
                Assert.IsTrue(first.Succeeded);
                ProbeSpawnedObject probe = first.Instance.GetComponent<ProbeSpawnedObject>();
                Assert.AreEqual(1, probe.SpawnedCount);
                Assert.AreEqual(2, probe.ResetCount); // warmup return plus spawn reuse
                DespawnResult despawn = service.Despawn(first.InstanceId, DespawnReason.Completed);
                Assert.IsTrue(despawn.Succeeded);
                Assert.AreEqual(2, probe.DespawnedCount);
                SpawnResult second = service.Spawn(Request(EnemyA, ChannelA, 2));
                Assert.AreSame(first.Instance, second.Instance);
                Assert.AreEqual(2, probe.SpawnedCount);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefab); }
        }

        [Test]
        public void DeterministicRequestOrder_MultipleSpawnablesAndChannels_AreTracked()
        {
            GameObject prefabA = Prefab("order-a");
            GameObject prefabB = Prefab("order-b");
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefabA, 2, 4), Def(EnemyB, prefabB, 2, 4) });
            try
            {
                service.Warmup();
                SpawnRequest[] requests =
                {
                    Request(EnemyB, ChannelB, 20),
                    Request(EnemyA, ChannelA, 10),
                    Request(EnemyA, ChannelB, 15)
                };
                SpawnResult[] results = new SpawnResult[3];
                service.SpawnMany(requests, 3, results);
                Assert.AreEqual(10, results[0].Request.Sequence);
                Assert.AreEqual(15, results[1].Request.Sequence);
                Assert.AreEqual(20, results[2].Request.Sequence);
                WorldSpawnSnapshot snapshot = service.CreateSnapshot();
                Assert.AreEqual(3, snapshot.ActiveInstances.Count);
                Assert.AreEqual(2, snapshot.Pools.Count);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefabA); UnityEngine.Object.DestroyImmediate(prefabB); }
        }

        [Test]
        public void DespawnUnknownAlreadyDespawnedAndClear_ReportExplicitly()
        {
            GameObject prefab = Prefab("despawn-prefab");
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefab, 0, 2) });
            try
            {
                Assert.AreEqual(DespawnFailureReason.UnknownInstance, service.Despawn(new SpawnInstanceId(99), DespawnReason.Requested).FailureReason);
                SpawnResult spawn = service.Spawn(Request(EnemyA));
                GameObject instance = spawn.Instance;
                Assert.IsTrue(service.Despawn(spawn.InstanceId, DespawnReason.Requested).Succeeded);
                instance.SetActive(false);
                Assert.AreEqual(DespawnFailureReason.UnknownInstance, service.Despawn(spawn.InstanceId, DespawnReason.Requested).FailureReason);
                SpawnResult again = service.Spawn(Request(EnemyA, ChannelA, 2));
                again.Instance.SetActive(false);
                Assert.AreEqual(DespawnFailureReason.AlreadyDespawned, service.Despawn(again.InstanceId, DespawnReason.Requested).FailureReason);
                service.Clear();
                Assert.AreEqual(0, service.ActiveCount);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefab); }
        }

        [Test]
        public void EncounterCompatibilityHarness_DrainsRequestsAndSpawnsPooledObjects()
        {
            GameObject prefab = Prefab("encounter-prefab");
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefab, 4, 4) });
            EncounterRuntime encounter = new EncounterRuntime(new EncounterDefinition(
                Encounter,
                Array.Empty<WeightedSpawnTableDefinition>(),
                new[] { new WaveDefinition(Wave, 0, new[] { SpawnGroupDefinition.Fixed(Group, EnemyA, 2, 2, 0, 1, ChannelA) }) },
                new[] { ObjectiveDefinition.AllWavesEmitted(new EncounterObjectiveId("objective.emitted")) }));
            try
            {
                service.Warmup();
                encounter.Start();
                encounter.AdvanceTicks(1);
                SpawnRequest[] buffer = new SpawnRequest[4];
                int count = encounter.DrainSpawnRequests(buffer).Written;
                SpawnResult[] results = new SpawnResult[4];
                service.SpawnMany(buffer, count, results);
                int externalActiveMetric = service.ActiveCount;
                Assert.AreEqual(2, externalActiveMetric);
                Assert.AreEqual(EncounterLifecycleState.Completed, encounter.State);
                Assert.AreEqual(Vector3.right, results[0].Instance.transform.position);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefab); }
        }

        [Test]
        public void DonorIdleAndTowerDefenseProofs_MapThroughResolversAndProviders()
        {
            GameObject donorPrefab = Prefab("donor-ghoul-prefab");
            WorldSpawnService donorService = Service(new[] { Def(new SpawnableId("enemy.ghoul-runner"), donorPrefab, 1, 2) });
            try
            {
                SpawnResult donor = donorService.Spawn(Request(new SpawnableId("enemy.ghoul-runner"), ChannelA));
                Assert.IsTrue(donor.Succeeded);
                Assert.IsTrue(donorService.Despawn(donor.InstanceId, DespawnReason.OutOfBounds).Succeeded);
            }
            finally { donorService.Dispose(); UnityEngine.Object.DestroyImmediate(donorPrefab); }

            GameObject idlePrefab = Prefab("idle-raider");
            WorldSpawnService idle = Service(new[] { Def(new SpawnableId("raider.basic"), idlePrefab, 1, 2) }, new Dictionary<SpawnChannelId, SpawnPose>
            {
                [new SpawnChannelId("perimeter-north")] = new SpawnPose(new Vector3(0, 0, 10), Quaternion.identity),
                [new SpawnChannelId("perimeter-random")] = new SpawnPose(new Vector3(3, 0, 8), Quaternion.identity)
            });
            try { Assert.IsTrue(idle.Spawn(Request(new SpawnableId("raider.basic"), new SpawnChannelId("perimeter-north"))).Succeeded); }
            finally { idle.Dispose(); UnityEngine.Object.DestroyImmediate(idlePrefab); }

            GameObject tdPrefab = Prefab("tower-creep");
            WorldSpawnService td = Service(new[] { Def(new SpawnableId("creep.light"), tdPrefab, 1, 2) }, new Dictionary<SpawnChannelId, SpawnPose>
            {
                [new SpawnChannelId("lane-a-entry")] = new SpawnPose(new Vector3(-5, 0, 0), Quaternion.identity),
                [new SpawnChannelId("lane-b-entry")] = new SpawnPose(new Vector3(5, 0, 0), Quaternion.identity)
            });
            try { Assert.AreEqual(-5f, td.Spawn(Request(new SpawnableId("creep.light"), new SpawnChannelId("lane-a-entry"))).Instance.transform.position.x); }
            finally { td.Dispose(); UnityEngine.Object.DestroyImmediate(tdPrefab); }
        }

        [Test]
        public void DurableBenchmark_WritesPooledSpawnDespawnMeasurements()
        {
            BenchmarkMeasurement one = MeasureCycles(1000);
            BenchmarkMeasurement five = MeasureCycles(5000);
            BenchmarkMeasurement ten = MeasureCycles(10000);
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logDirectory);
            string path = Path.Combine(logDirectory, "world-spawning-benchmark-results.json");
            File.WriteAllText(path, BuildBenchmarkJson(one, five, ten), Encoding.UTF8);
            TestContext.WriteLine(path);
            Assert.AreEqual(1000, one.OperationCount);
            Assert.AreEqual(5000, five.OperationCount);
            Assert.AreEqual(10000, ten.OperationCount);
        }

        [Test]
        public void RepresentativeWarmPoolSpawnDespawn_DoesNotInstantiateOrDestroyAfterWarmup()
        {
            GameObject prefab = Prefab("hot-prefab");
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefab, 2, 2) });
            try
            {
                service.Warmup();
                int totalBefore = service.CreateSnapshot().Pools[0].TotalCount;
                SpawnResult result = service.Spawn(Request(EnemyA));
                service.Despawn(result.InstanceId, DespawnReason.Requested);
                int totalAfter = service.CreateSnapshot().Pools[0].TotalCount;
                Assert.AreEqual(totalBefore, totalAfter);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefab); }
        }

        private static BenchmarkMeasurement MeasureCycles(int count)
        {
            GameObject prefab = Prefab("benchmark-prefab-" + count);
            WorldSpawnService service = Service(new[] { Def(EnemyA, prefab, count, count) });
            try
            {
                service.Warmup();
                SpawnRequest request = Request(EnemyA);
                long beforeBytes = GC.GetAllocatedBytesForCurrentThread();
                Stopwatch stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < count; i++)
                {
                    SpawnResult result = service.Spawn(request);
                    service.Despawn(result.InstanceId, DespawnReason.Requested);
                }
                stopwatch.Stop();
                long bytes = GC.GetAllocatedBytesForCurrentThread() - beforeBytes;
                return new BenchmarkMeasurement(count, stopwatch.Elapsed.TotalMilliseconds, bytes);
            }
            finally { service.Dispose(); UnityEngine.Object.DestroyImmediate(prefab); }
        }

        private static string BuildBenchmarkJson(params BenchmarkMeasurement[] measurements)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"unityVersion\": \"6000.3.5f1\",");
            builder.AppendLine("  \"runtime\": \"Unity EditMode Mono\",");
            builder.AppendLine("  \"configuration\": \"world-spawning-phase-1f-pooled-cycles\",");
            builder.AppendLine("  \"poolWarmup\": \"initialCapacity equals operation count, maximumCapacity equals operation count\",");
            builder.AppendLine("  \"prefabComplexity\": \"single empty GameObject\",");
            builder.AppendLine("  \"measurements\": [");
            for (int i = 0; i < measurements.Length; i++)
            {
                BenchmarkMeasurement m = measurements[i];
                builder.Append("    { \"operationCount\": ").Append(m.OperationCount)
                    .Append(", \"elapsedMs\": ").Append(m.ElapsedMs.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(", \"bytesAllocated\": ").Append(m.BytesAllocated)
                    .Append(" }");
                builder.AppendLine(i + 1 == measurements.Length ? string.Empty : ",");
            }
            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static WorldSpawnService Service(IReadOnlyList<SpawnableDefinition> definitions, IReadOnlyDictionary<SpawnChannelId, SpawnPose> poses = null)
        {
            poses ??= new Dictionary<SpawnChannelId, SpawnPose>
            {
                [ChannelA] = new SpawnPose(Vector3.right, Quaternion.identity),
                [ChannelB] = new SpawnPose(Vector3.left, Quaternion.identity)
            };
            return new WorldSpawnService(new SpawnableCatalog(definitions), new ChannelPoseResolver(poses));
        }

        private static SpawnableDefinition Def(SpawnableId id, GameObject prefab, int initial = 0, int max = 8)
        {
            return new SpawnableDefinition(id, new GameObjectPrefabProvider(prefab), initial, max);
        }

        private static SpawnRequest Request(SpawnableId id, SpawnChannelId channel = default, long sequence = 1)
        {
            if (channel.IsEmpty) channel = ChannelA;
            return new SpawnRequest(Encounter, Wave, Group, id, channel, 0, sequence, 0, sequence);
        }

        private static GameObject Prefab(string name)
        {
            GameObject prefab = new GameObject(name);
            prefab.SetActive(false);
            return prefab;
        }

        private readonly struct BenchmarkMeasurement
        {
            public BenchmarkMeasurement(int operationCount, double elapsedMs, long bytesAllocated) { OperationCount = operationCount; ElapsedMs = elapsedMs; BytesAllocated = bytesAllocated; }
            public int OperationCount { get; }
            public double ElapsedMs { get; }
            public long BytesAllocated { get; }
        }
    }

    public sealed class ProbeSpawnedObject : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        public int SpawnedCount { get; private set; }
        public int DespawnedCount { get; private set; }
        public int ResetCount { get; private set; }
        public void OnWorldSpawned(WorldSpawnContext context) { SpawnedCount++; }
        public void OnWorldDespawned(DespawnReason reason) { DespawnedCount++; }
        public void ResetForWorldSpawn() { ResetCount++; }
    }
}
