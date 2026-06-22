# Donor Integration Findings

Primary donor:

`C:/Repositories/JorisHoef/Codex-Attempted-Vampire-Project/Codex-Attempted-Vampire-Project`

## Current Donor Behavior

- `WaveSpawnDirector` selects enemies from weighted wave segment data and filters by level theme.
- `WaveSpawnDirector` computes radial positions around the player using random angle and configured spawn distance.
- `GameSessionBootstrapper.SpawnEnemy` resolves the enemy prefab, gets an object from `GameObjectPoolService`, positions it, and initializes `EnemyActor`.
- `GameObjectPoolService` has prefab buckets, inactive stacks, prewarm, reuse, and release.
- The donor pool grows when empty and does not expose max-capacity failure results.
- Enemy despawn/death calls back into `GameSessionBootstrapper.ReleaseEnemy`, which returns the GameObject to the pool.

## Mapping Proof

- donor enemy id such as `enemy.ghoul-runner` maps to `SpawnableId`
- donor radial spawn logic maps to an `ISpawnPoseResolver`
- donor default or override prefab maps to `ISpawnPrefabProvider`
- one `SpawnRequest` creates one pooled instance
- `Despawn` returns the instance to the pool

## Outside Package

Movement, combat, health, status effects, damage numbers, enemy registry simulation, rewards, UI, run escalation, and level filtering stay outside World Spawning.
