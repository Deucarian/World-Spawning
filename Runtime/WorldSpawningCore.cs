using System;
using System.Collections.Generic;
using Deucarian.Encounters;
using UnityEngine;

namespace Deucarian.WorldSpawning
{
    public readonly struct SpawnInstanceId : IEquatable<SpawnInstanceId>, IComparable<SpawnInstanceId>
    {
        public SpawnInstanceId(long value) { if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value)); Value = value; }
        public long Value { get; }
        public bool Equals(SpawnInstanceId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SpawnInstanceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(SpawnInstanceId other) => Value.CompareTo(other.Value);
        public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public readonly struct SpawnPose
    {
        public SpawnPose(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Position = position; Rotation = rotation; Parent = parent;
        }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Transform Parent { get; }
    }

    public enum SpawnFailureReason { None = 0, InvalidRequest = 1, UnknownSpawnable = 2, MissingPrefabProvider = 3, InvalidPrefab = 4, UnknownChannel = 5, PoseResolutionFailed = 6, CapacityExhausted = 7, DuplicateInstanceId = 8, PoolFailure = 9 }
    public enum DespawnReason { Requested = 0, Completed = 1, Killed = 2, OutOfBounds = 3, Replaced = 4, Clear = 5 }
    public enum DespawnFailureReason { None = 0, InvalidInstanceId = 1, UnknownInstance = 2, AlreadyDespawned = 3 }

    public readonly struct SpawnPoseResult
    {
        private SpawnPoseResult(bool succeeded, SpawnPose pose, string message) { Succeeded = succeeded; Pose = pose; Message = message ?? string.Empty; }
        public bool Succeeded { get; }
        public SpawnPose Pose { get; }
        public string Message { get; }
        public static SpawnPoseResult Success(SpawnPose pose) => new SpawnPoseResult(true, pose, string.Empty);
        public static SpawnPoseResult Failure(string message) => new SpawnPoseResult(false, default, message);
    }

    public interface ISpawnPoseResolver
    {
        SpawnPoseResult TryResolvePose(SpawnRequest request);
    }

    public interface ISpawnPrefabProvider
    {
        GameObject GetPrefab(SpawnableId spawnableId);
    }

    public sealed class GameObjectPrefabProvider : ISpawnPrefabProvider
    {
        private readonly GameObject _prefab;
        public GameObjectPrefabProvider(GameObject prefab) { _prefab = prefab; }
        public GameObject GetPrefab(SpawnableId spawnableId) => _prefab;
    }

    public interface IWorldSpawnedObject
    {
        void OnWorldSpawned(WorldSpawnContext context);
        void OnWorldDespawned(DespawnReason reason);
    }

    public interface IWorldSpawnResettable
    {
        void ResetForWorldSpawn();
    }

    public readonly struct WorldSpawnContext
    {
        public WorldSpawnContext(SpawnInstanceId instanceId, SpawnRequest request, SpawnPose pose)
        {
            InstanceId = instanceId; Request = request; Pose = pose;
        }
        public SpawnInstanceId InstanceId { get; }
        public SpawnRequest Request { get; }
        public SpawnPose Pose { get; }
    }

    public sealed class SpawnableDefinition
    {
        public SpawnableDefinition(SpawnableId id, ISpawnPrefabProvider prefabProvider, int initialCapacity = 0, int maximumCapacity = 64, string poolRootName = null)
        {
            if (id.IsEmpty) throw new ArgumentException("Spawnable id cannot be empty.", nameof(id));
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            if (maximumCapacity <= 0 || maximumCapacity < initialCapacity) throw new ArgumentOutOfRangeException(nameof(maximumCapacity));
            Id = id; PrefabProvider = prefabProvider; InitialCapacity = initialCapacity; MaximumCapacity = maximumCapacity; PoolRootName = string.IsNullOrWhiteSpace(poolRootName) ? id.Value + "-pool" : poolRootName;
        }
        public SpawnableId Id { get; }
        public ISpawnPrefabProvider PrefabProvider { get; }
        public int InitialCapacity { get; }
        public int MaximumCapacity { get; }
        public string PoolRootName { get; }
    }

    public sealed class SpawnableCatalog
    {
        private readonly Dictionary<SpawnableId, SpawnableDefinition> _definitions = new Dictionary<SpawnableId, SpawnableDefinition>();
        public SpawnableCatalog(IReadOnlyList<SpawnableDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            for (int i = 0; i < definitions.Count; i++)
            {
                SpawnableDefinition definition = definitions[i] ?? throw new ArgumentException("Spawnable definition cannot be null.");
                if (_definitions.ContainsKey(definition.Id)) throw new ArgumentException("Duplicate spawnable id: " + definition.Id);
                _definitions.Add(definition.Id, definition);
            }
        }
        public bool TryGet(SpawnableId id, out SpawnableDefinition definition) => _definitions.TryGetValue(id, out definition);
        public SpawnableDefinition[] GetDefinitionsOrdered()
        {
            var result = new SpawnableDefinition[_definitions.Count];
            _definitions.Values.CopyTo(result, 0);
            Array.Sort(result, (left, right) => left.Id.CompareTo(right.Id));
            return result;
        }
    }

    public sealed class ChannelPoseResolver : ISpawnPoseResolver
    {
        private readonly Dictionary<SpawnChannelId, SpawnPose> _poses = new Dictionary<SpawnChannelId, SpawnPose>();
        public ChannelPoseResolver(IReadOnlyDictionary<SpawnChannelId, SpawnPose> poses)
        {
            if (poses == null) throw new ArgumentNullException(nameof(poses));
            foreach (KeyValuePair<SpawnChannelId, SpawnPose> pair in poses) _poses.Add(pair.Key, pair.Value);
        }
        public SpawnPoseResult TryResolvePose(SpawnRequest request)
        {
            return _poses.TryGetValue(request.ChannelId, out SpawnPose pose)
                ? SpawnPoseResult.Success(pose)
                : SpawnPoseResult.Failure("Unknown spawn channel: " + request.ChannelId);
        }
    }

    public readonly struct SpawnResult
    {
        public SpawnResult(bool succeeded, SpawnFailureReason failureReason, SpawnInstanceId instanceId, GameObject instance, SpawnRequest request, string message = null)
        {
            Succeeded = succeeded; FailureReason = failureReason; InstanceId = instanceId; Instance = instance; Request = request; Message = message ?? string.Empty;
        }
        public bool Succeeded { get; }
        public SpawnFailureReason FailureReason { get; }
        public SpawnInstanceId InstanceId { get; }
        public GameObject Instance { get; }
        public SpawnRequest Request { get; }
        public string Message { get; }
    }

    public readonly struct DespawnResult
    {
        public DespawnResult(bool succeeded, DespawnFailureReason failureReason, SpawnInstanceId instanceId, GameObject instance, DespawnReason reason, string message = null)
        {
            Succeeded = succeeded; FailureReason = failureReason; InstanceId = instanceId; Instance = instance; Reason = reason; Message = message ?? string.Empty;
        }
        public bool Succeeded { get; }
        public DespawnFailureReason FailureReason { get; }
        public SpawnInstanceId InstanceId { get; }
        public GameObject Instance { get; }
        public DespawnReason Reason { get; }
        public string Message { get; }
    }

    public readonly struct SpawnPoolSnapshot
    {
        public SpawnPoolSnapshot(SpawnableId spawnableId, int activeCount, int pooledCount, int totalCount, int maximumCapacity)
        {
            SpawnableId = spawnableId; ActiveCount = activeCount; PooledCount = pooledCount; TotalCount = totalCount; MaximumCapacity = maximumCapacity;
        }
        public SpawnableId SpawnableId { get; }
        public int ActiveCount { get; }
        public int PooledCount { get; }
        public int TotalCount { get; }
        public int MaximumCapacity { get; }
    }

    public readonly struct ActiveSpawnSnapshot
    {
        public ActiveSpawnSnapshot(SpawnInstanceId instanceId, SpawnableId spawnableId, SpawnChannelId channelId, long requestSequence)
        {
            InstanceId = instanceId; SpawnableId = spawnableId; ChannelId = channelId; RequestSequence = requestSequence;
        }
        public SpawnInstanceId InstanceId { get; }
        public SpawnableId SpawnableId { get; }
        public SpawnChannelId ChannelId { get; }
        public long RequestSequence { get; }
    }

    public sealed class WorldSpawnSnapshot
    {
        public WorldSpawnSnapshot(IReadOnlyList<SpawnPoolSnapshot> pools, IReadOnlyList<ActiveSpawnSnapshot> activeInstances)
        {
            Pools = Copy(pools); ActiveInstances = Copy(activeInstances);
        }
        public IReadOnlyList<SpawnPoolSnapshot> Pools { get; }
        public IReadOnlyList<ActiveSpawnSnapshot> ActiveInstances { get; }
        private static T[] Copy<T>(IReadOnlyList<T> source) { if (source == null) return Array.Empty<T>(); var copy = new T[source.Count]; for (int i = 0; i < source.Count; i++) copy[i] = source[i]; return copy; }
    }

    public interface IWorldSpawnPool : IDisposable
    {
        void Warmup(SpawnableDefinition definition, Transform poolRoot);
        bool TrySpawn(SpawnableDefinition definition, SpawnPose pose, out GameObject instance, out SpawnFailureReason reason);
        bool TryDespawn(GameObject instance, Transform poolRoot, DespawnReason reason);
        void Clear(bool destroyInstances);
        SpawnPoolSnapshot[] CreateSnapshots();
    }

    public sealed class GameObjectWorldSpawnPool : IWorldSpawnPool
    {
        private readonly Dictionary<SpawnableId, PoolBucket> _buckets = new Dictionary<SpawnableId, PoolBucket>();
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
                        if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
                    }
                    if (bucket.Root != null) UnityEngine.Object.DestroyImmediate(bucket.Root.gameObject);
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

    public sealed class WorldSpawnService : IDisposable
    {
        private readonly SpawnableCatalog _catalog;
        private readonly ISpawnPoseResolver _poseResolver;
        private readonly IWorldSpawnPool _pool;
        private readonly Transform _root;
        private readonly Dictionary<SpawnInstanceId, ActiveRecord> _active = new Dictionary<SpawnInstanceId, ActiveRecord>();
        private long _nextInstanceId;
        private bool _disposed;

        public WorldSpawnService(SpawnableCatalog catalog, ISpawnPoseResolver poseResolver, Transform root = null, IWorldSpawnPool pool = null, string rootName = "WorldSpawning")
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _poseResolver = poseResolver ?? throw new ArgumentNullException(nameof(poseResolver));
            _pool = pool ?? new GameObjectWorldSpawnPool();
            if (root == null)
            {
                GameObject rootObject = new GameObject(string.IsNullOrWhiteSpace(rootName) ? "WorldSpawning" : rootName);
                _root = rootObject.transform;
            }
            else
            {
                _root = root;
            }
        }

        public int ActiveCount => _active.Count;

        public void Warmup()
        {
            foreach (SpawnableDefinition definition in _catalog.GetDefinitionsOrdered())
            {
                ValidateProvider(definition);
                _pool.Warmup(definition, _root);
            }
        }

        public SpawnResult Spawn(SpawnRequest request)
        {
            if (_disposed) return Failure(request, SpawnFailureReason.PoolFailure, "Service is disposed.");
            SpawnFailureReason validation = ValidateRequest(request);
            if (validation != SpawnFailureReason.None) return Failure(request, validation, "Invalid spawn request.");
            if (!_catalog.TryGet(request.SpawnableId, out SpawnableDefinition definition)) return Failure(request, SpawnFailureReason.UnknownSpawnable, "Unknown spawnable: " + request.SpawnableId);
            SpawnFailureReason providerFailure = ValidateProvider(definition);
            if (providerFailure != SpawnFailureReason.None) return Failure(request, providerFailure, "Invalid prefab provider.");
            SpawnPoseResult pose = _poseResolver.TryResolvePose(request);
            if (!pose.Succeeded) return Failure(request, request.ChannelId.IsEmpty ? SpawnFailureReason.InvalidRequest : SpawnFailureReason.PoseResolutionFailed, pose.Message);
            if (!_pool.TrySpawn(definition, pose.Pose, out GameObject instance, out SpawnFailureReason poolReason)) return Failure(request, poolReason, "Pool failed to spawn.");
            var id = new SpawnInstanceId(++_nextInstanceId);
            if (_active.ContainsKey(id))
            {
                _pool.TryDespawn(instance, _root, DespawnReason.Clear);
                return Failure(request, SpawnFailureReason.DuplicateInstanceId, "Duplicate instance id.");
            }
            var record = new ActiveRecord(id, request, instance);
            _active.Add(id, record);
            GameObjectWorldSpawnPool.NotifySpawned(instance, new WorldSpawnContext(id, request, pose.Pose));
            return new SpawnResult(true, SpawnFailureReason.None, id, instance, request);
        }

        public int SpawnMany(SpawnRequest[] requests, int count, SpawnResult[] results)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));
            if (results == null) throw new ArgumentNullException(nameof(results));
            if (count < 0 || count > requests.Length || count > results.Length) throw new ArgumentOutOfRangeException(nameof(count));
            Array.Sort(requests, 0, count, SpawnRequestSequenceComparer.Instance);
            for (int i = 0; i < count; i++) results[i] = Spawn(requests[i]);
            return count;
        }

        public DespawnResult Despawn(SpawnInstanceId instanceId, DespawnReason reason)
        {
            if (instanceId.Value <= 0) return new DespawnResult(false, DespawnFailureReason.InvalidInstanceId, instanceId, null, reason);
            if (!_active.TryGetValue(instanceId, out ActiveRecord record)) return new DespawnResult(false, DespawnFailureReason.UnknownInstance, instanceId, null, reason);
            if (record.Instance == null || !record.Instance.activeSelf)
            {
                _active.Remove(instanceId);
                return new DespawnResult(false, DespawnFailureReason.AlreadyDespawned, instanceId, record.Instance, reason);
            }
            _pool.TryDespawn(record.Instance, _root, reason);
            _active.Remove(instanceId);
            return new DespawnResult(true, DespawnFailureReason.None, instanceId, record.Instance, reason);
        }

        public bool TryGetActiveInstance(SpawnInstanceId id, out GameObject instance)
        {
            if (_active.TryGetValue(id, out ActiveRecord record)) { instance = record.Instance; return true; }
            instance = null; return false;
        }

        public WorldSpawnSnapshot CreateSnapshot()
        {
            var active = new ActiveSpawnSnapshot[_active.Count];
            int index = 0;
            foreach (ActiveRecord record in _active.Values)
            {
                active[index++] = new ActiveSpawnSnapshot(record.Id, record.Request.SpawnableId, record.Request.ChannelId, record.Request.Sequence);
            }
            Array.Sort(active, (left, right) => left.InstanceId.CompareTo(right.InstanceId));
            return new WorldSpawnSnapshot(_pool.CreateSnapshots(), active);
        }

        public void Clear(bool destroyInstances = true)
        {
            var ids = new SpawnInstanceId[_active.Count];
            _active.Keys.CopyTo(ids, 0);
            for (int i = 0; i < ids.Length; i++) Despawn(ids[i], DespawnReason.Clear);
            _pool.Clear(destroyInstances);
            _active.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            Clear(true);
            _pool.Dispose();
            if (_root != null) UnityEngine.Object.DestroyImmediate(_root.gameObject);
            _disposed = true;
        }

        private static SpawnFailureReason ValidateProvider(SpawnableDefinition definition)
        {
            if (definition.PrefabProvider == null) return SpawnFailureReason.MissingPrefabProvider;
            GameObject prefab = definition.PrefabProvider.GetPrefab(definition.Id);
            return prefab == null ? SpawnFailureReason.InvalidPrefab : SpawnFailureReason.None;
        }

        private static SpawnFailureReason ValidateRequest(SpawnRequest request)
        {
            if (request.SpawnableId.IsEmpty || request.ChannelId.IsEmpty || request.GroupId.IsEmpty || request.WaveId.IsEmpty || request.EncounterId.IsEmpty) return SpawnFailureReason.InvalidRequest;
            return SpawnFailureReason.None;
        }

        private static SpawnResult Failure(SpawnRequest request, SpawnFailureReason reason, string message) => new SpawnResult(false, reason, default, null, request, message);

        private readonly struct ActiveRecord
        {
            public ActiveRecord(SpawnInstanceId id, SpawnRequest request, GameObject instance) { Id = id; Request = request; Instance = instance; }
            public SpawnInstanceId Id { get; }
            public SpawnRequest Request { get; }
            public GameObject Instance { get; }
        }

        private sealed class SpawnRequestSequenceComparer : IComparer<SpawnRequest>
        {
            public static readonly SpawnRequestSequenceComparer Instance = new SpawnRequestSequenceComparer();
            public int Compare(SpawnRequest x, SpawnRequest y)
            {
                int sequence = x.Sequence.CompareTo(y.Sequence);
                if (sequence != 0) return sequence;
                int wave = x.WaveId.CompareTo(y.WaveId);
                return wave != 0 ? wave : x.GroupId.CompareTo(y.GroupId);
            }
        }
    }
}
