using System;
using System.Collections.Generic;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.WorldSpawning
{
    public sealed class GameObjectWorldSpawnPool : IWorldSpawnPool
    {
        private readonly Dictionary<WorldSpawnableId, PoolBucket> _buckets = new Dictionary<WorldSpawnableId, PoolBucket>();
        private readonly Dictionary<GameObject, PoolBucket> _bucketByInstance = new Dictionary<GameObject, PoolBucket>();

        public void Warmup(SpawnableDefinition definition, Transform poolRoot)
        {
            PoolBucket bucket = GetOrCreateBucket(definition, poolRoot);
            while (bucket.TotalCount < definition.InitialCapacity)
            {
                GameObject instance = CreateInstance(bucket);
                ReturnToPool(bucket, instance, DespawnReason.Clear);
            }
        }

        public bool TrySpawn(SpawnableDefinition definition, SpawnPose pose, out GameObject instance, out SpawnFailureReason reason)
        {
            instance = null; reason = SpawnFailureReason.None;
            PoolBucket bucket = GetOrCreateBucket(definition, null);
            if (bucket.Inactive.Count > 0)
            {
                instance = bucket.Inactive.Pop();
            }
            else
            {
                if (bucket.TotalCount >= definition.MaximumCapacity)
                {
                    reason = SpawnFailureReason.CapacityExhausted;
                    return false;
                }
                instance = CreateInstance(bucket);
            }
            bucket.ActiveCount++;
            Reset(instance);
            Transform t = instance.transform;
            t.SetParent(pose.Parent, true);
            t.SetPositionAndRotation(pose.Position, pose.Rotation);
            instance.SetActive(true);
            return true;
        }

        public bool TryDespawn(GameObject instance, Transform poolRoot, DespawnReason reason)
        {
            if (instance == null || !_bucketByInstance.TryGetValue(instance, out PoolBucket bucket)) return false;
            if (!instance.activeSelf && instance.transform.parent == bucket.Root) return true;
            bucket.ActiveCount = Math.Max(0, bucket.ActiveCount - 1);
            ReturnToPool(bucket, instance, reason);
            return true;
        }

        public void Clear(bool destroyInstances)
        {
            foreach (PoolBucket bucket in _buckets.Values)
            {
                if (destroyInstances)
                {
                    foreach (GameObject instance in bucket.AllInstances)
                    {
                        UnityObjectUtility.DestroySafely(instance);
                    }
                    if (bucket.Root != null) UnityObjectUtility.DestroySafely(bucket.Root.gameObject);
                }
                bucket.Inactive.Clear();
                bucket.AllInstances.Clear();
                bucket.ActiveCount = 0;
            }
            _bucketByInstance.Clear();
            _buckets.Clear();
        }

        public SpawnPoolSnapshot[] CreateSnapshots()
        {
            var snapshots = new SpawnPoolSnapshot[_buckets.Count];
            int index = 0;
            foreach (PoolBucket bucket in _buckets.Values)
            {
                snapshots[index++] = new SpawnPoolSnapshot(bucket.Definition.Id, bucket.ActiveCount, bucket.Inactive.Count, bucket.TotalCount, bucket.Definition.MaximumCapacity);
            }
            Array.Sort(snapshots, (left, right) => left.SpawnableId.CompareTo(right.SpawnableId));
            return snapshots;
        }

        public void Dispose() => Clear(true);

        private PoolBucket GetOrCreateBucket(SpawnableDefinition definition, Transform poolRoot)
        {
            if (_buckets.TryGetValue(definition.Id, out PoolBucket bucket)) return bucket;
            GameObject prefab = definition.PrefabProvider.GetPrefab(definition.Id);
            GameObject rootObject = new GameObject(definition.PoolRootName);
            if (poolRoot != null) rootObject.transform.SetParent(poolRoot, false);
            bucket = new PoolBucket(definition, prefab, rootObject.transform);
            _buckets.Add(definition.Id, bucket);
            return bucket;
        }

        private GameObject CreateInstance(PoolBucket bucket)
        {
            GameObject instance = UnityEngine.Object.Instantiate(bucket.Prefab, bucket.Root);
            instance.SetActive(false);
            bucket.AllInstances.Add(instance);
            _bucketByInstance.Add(instance, bucket);
            return instance;
        }

        private static void ReturnToPool(PoolBucket bucket, GameObject instance, DespawnReason reason)
        {
            NotifyDespawned(instance, reason);
            Reset(instance);
            Transform transform = instance.transform;
            transform.SetParent(bucket.Root, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            instance.SetActive(false);
            bucket.Inactive.Push(instance);
        }

        private static void Reset(GameObject instance)
        {
            if (instance == null) return;
            IWorldSpawnResettable[] resettable = instance.GetComponentsInChildren<IWorldSpawnResettable>(true);
            for (int i = 0; i < resettable.Length; i++) resettable[i].ResetForWorldSpawn();
        }

        internal static void NotifySpawned(GameObject instance, WorldSpawnContext context)
        {
            IWorldSpawnedObject[] spawned = instance.GetComponentsInChildren<IWorldSpawnedObject>(true);
            for (int i = 0; i < spawned.Length; i++) spawned[i].OnWorldSpawned(context);
        }

        internal static void NotifyDespawned(GameObject instance, DespawnReason reason)
        {
            IWorldSpawnedObject[] spawned = instance.GetComponentsInChildren<IWorldSpawnedObject>(true);
            for (int i = 0; i < spawned.Length; i++) spawned[i].OnWorldDespawned(reason);
        }

        private sealed class PoolBucket
        {
            public PoolBucket(SpawnableDefinition definition, GameObject prefab, Transform root) { Definition = definition; Prefab = prefab; Root = root; }
            public SpawnableDefinition Definition { get; }
            public GameObject Prefab { get; }
            public Transform Root { get; }
            public Stack<GameObject> Inactive { get; } = new Stack<GameObject>();
            public List<GameObject> AllInstances { get; } = new List<GameObject>();
            public int ActiveCount { get; set; }
            public int TotalCount => ActiveCount + Inactive.Count;
        }
    }
}
