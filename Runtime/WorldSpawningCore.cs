using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.GameplayFoundation;
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

    public readonly struct WorldSpawnableId : IEquatable<WorldSpawnableId>, IComparable<WorldSpawnableId>
    {
        private readonly ContentId _value;
        public WorldSpawnableId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(WorldSpawnableId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is WorldSpawnableId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(WorldSpawnableId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public readonly struct WorldSpawnChannelId : IEquatable<WorldSpawnChannelId>, IComparable<WorldSpawnChannelId>
    {
        private readonly ContentId _value;
        public WorldSpawnChannelId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(WorldSpawnChannelId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is WorldSpawnChannelId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(WorldSpawnChannelId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public readonly struct WorldSpawnRequestContext
    {
        public WorldSpawnRequestContext(string sourceSystem = null, string encounterId = null, string waveId = null, string groupId = null, int groupIndex = 0, int tick = 0)
        {
            SourceSystem = sourceSystem ?? string.Empty; EncounterId = encounterId ?? string.Empty; WaveId = waveId ?? string.Empty; GroupId = groupId ?? string.Empty; GroupIndex = groupIndex; Tick = tick;
        }
        public string SourceSystem { get; }
        public string EncounterId { get; }
        public string WaveId { get; }
        public string GroupId { get; }
        public int GroupIndex { get; }
        public int Tick { get; }
    }

    public readonly struct WorldSpawnRequest
    {
        public WorldSpawnRequest(WorldSpawnableId spawnableId, WorldSpawnChannelId channelId, long sequence, WorldSpawnRequestContext context = default)
        {
            SpawnableId = spawnableId; ChannelId = channelId; Sequence = sequence; Context = context;
        }
        public WorldSpawnableId SpawnableId { get; }
        public WorldSpawnChannelId ChannelId { get; }
        public long Sequence { get; }
        public WorldSpawnRequestContext Context { get; }
    }

    public interface IWorldSpawnRequestAdapter<in TSource>
    {
        WorldSpawnRequest Convert(TSource source);
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
        SpawnPoseResult TryResolvePose(WorldSpawnRequest request);
    }

    public interface ISpawnPrefabProvider
    {
        GameObject GetPrefab(WorldSpawnableId spawnableId);
    }

    public sealed class GameObjectPrefabProvider : ISpawnPrefabProvider
    {
        private readonly GameObject _prefab;
        public GameObjectPrefabProvider(GameObject prefab) { _prefab = prefab; }
        public GameObject GetPrefab(WorldSpawnableId spawnableId) => _prefab;
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
        public WorldSpawnContext(SpawnInstanceId instanceId, WorldSpawnRequest request, SpawnPose pose)
        {
            InstanceId = instanceId; Request = request; Pose = pose;
        }
        public SpawnInstanceId InstanceId { get; }
        public WorldSpawnRequest Request { get; }
        public SpawnPose Pose { get; }
    }

    public sealed class SpawnableDefinition
    {
        public SpawnableDefinition(WorldSpawnableId id, ISpawnPrefabProvider prefabProvider, int initialCapacity = 0, int maximumCapacity = 64, string poolRootName = null)
        {
            if (id.IsEmpty) throw new ArgumentException("Spawnable id cannot be empty.", nameof(id));
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            if (maximumCapacity <= 0 || maximumCapacity < initialCapacity) throw new ArgumentOutOfRangeException(nameof(maximumCapacity));
            Id = id; PrefabProvider = prefabProvider; InitialCapacity = initialCapacity; MaximumCapacity = maximumCapacity; PoolRootName = string.IsNullOrWhiteSpace(poolRootName) ? id.Value + "-pool" : poolRootName;
        }
        public WorldSpawnableId Id { get; }
        public ISpawnPrefabProvider PrefabProvider { get; }
        public int InitialCapacity { get; }
        public int MaximumCapacity { get; }
        public string PoolRootName { get; }
    }

    public sealed class SpawnableCatalog
    {
        private readonly Dictionary<WorldSpawnableId, SpawnableDefinition> _definitions = new Dictionary<WorldSpawnableId, SpawnableDefinition>();
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
        public bool TryGet(WorldSpawnableId id, out SpawnableDefinition definition) => _definitions.TryGetValue(id, out definition);
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
        private readonly Dictionary<WorldSpawnChannelId, SpawnPose> _poses = new Dictionary<WorldSpawnChannelId, SpawnPose>();
        public ChannelPoseResolver(IReadOnlyDictionary<WorldSpawnChannelId, SpawnPose> poses)
        {
            if (poses == null) throw new ArgumentNullException(nameof(poses));
            foreach (KeyValuePair<WorldSpawnChannelId, SpawnPose> pair in poses) _poses.Add(pair.Key, pair.Value);
        }
        public SpawnPoseResult TryResolvePose(WorldSpawnRequest request)
        {
            return _poses.TryGetValue(request.ChannelId, out SpawnPose pose)
                ? SpawnPoseResult.Success(pose)
                : SpawnPoseResult.Failure("Unknown spawn channel: " + request.ChannelId);
        }
    }

    public readonly struct SpawnResult
    {
        public SpawnResult(bool succeeded, SpawnFailureReason failureReason, SpawnInstanceId instanceId, GameObject instance, WorldSpawnRequest request, string message = null)
        {
            Succeeded = succeeded; FailureReason = failureReason; InstanceId = instanceId; Instance = instance; Request = request; Message = message ?? string.Empty;
        }
        public bool Succeeded { get; }
        public SpawnFailureReason FailureReason { get; }
        public SpawnInstanceId InstanceId { get; }
        public GameObject Instance { get; }
        public WorldSpawnRequest Request { get; }
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
        public SpawnPoolSnapshot(WorldSpawnableId spawnableId, int activeCount, int pooledCount, int totalCount, int maximumCapacity)
        {
            SpawnableId = spawnableId; ActiveCount = activeCount; PooledCount = pooledCount; TotalCount = totalCount; MaximumCapacity = maximumCapacity;
        }
        public WorldSpawnableId SpawnableId { get; }
        public int ActiveCount { get; }
        public int PooledCount { get; }
        public int TotalCount { get; }
        public int MaximumCapacity { get; }
    }

    public readonly struct ActiveSpawnSnapshot
    {
        public ActiveSpawnSnapshot(SpawnInstanceId instanceId, WorldSpawnableId spawnableId, WorldSpawnChannelId channelId, long requestSequence)
        {
            InstanceId = instanceId; SpawnableId = spawnableId; ChannelId = channelId; RequestSequence = requestSequence;
        }
        public SpawnInstanceId InstanceId { get; }
        public WorldSpawnableId SpawnableId { get; }
        public WorldSpawnChannelId ChannelId { get; }
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

        public SpawnResult Spawn(WorldSpawnRequest request)
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

        public int SpawnMany(WorldSpawnRequest[] requests, int count, SpawnResult[] results)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));
            if (results == null) throw new ArgumentNullException(nameof(results));
            if (count < 0 || count > requests.Length || count > results.Length) throw new ArgumentOutOfRangeException(nameof(count));
            Array.Sort(requests, 0, count, WorldSpawnRequestSequenceComparer.Instance);
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
            if (_root != null) UnityObjectUtility.DestroySafely(_root.gameObject);
            _disposed = true;
        }

        private static SpawnFailureReason ValidateProvider(SpawnableDefinition definition)
        {
            if (definition.PrefabProvider == null) return SpawnFailureReason.MissingPrefabProvider;
            GameObject prefab = definition.PrefabProvider.GetPrefab(definition.Id);
            return prefab == null ? SpawnFailureReason.InvalidPrefab : SpawnFailureReason.None;
        }

        private static SpawnFailureReason ValidateRequest(WorldSpawnRequest request)
        {
            if (request.SpawnableId.IsEmpty || request.ChannelId.IsEmpty) return SpawnFailureReason.InvalidRequest;
            return SpawnFailureReason.None;
        }

        private static SpawnResult Failure(WorldSpawnRequest request, SpawnFailureReason reason, string message) => new SpawnResult(false, reason, default, null, request, message);

        private readonly struct ActiveRecord
        {
            public ActiveRecord(SpawnInstanceId id, WorldSpawnRequest request, GameObject instance) { Id = id; Request = request; Instance = instance; }
            public SpawnInstanceId Id { get; }
            public WorldSpawnRequest Request { get; }
            public GameObject Instance { get; }
        }

        private sealed class WorldSpawnRequestSequenceComparer : IComparer<WorldSpawnRequest>
        {
            public static readonly WorldSpawnRequestSequenceComparer Instance = new WorldSpawnRequestSequenceComparer();
            public int Compare(WorldSpawnRequest x, WorldSpawnRequest y)
            {
                int sequence = x.Sequence.CompareTo(y.Sequence);
                if (sequence != 0) return sequence;
                int channel = x.ChannelId.CompareTo(y.ChannelId);
                return channel != 0 ? channel : x.SpawnableId.CompareTo(y.SpawnableId);
            }
        }
    }
}
