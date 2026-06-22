# Reuse Findings

## Idle Auto Defense

Idle Auto Defense can use channels such as:

- `perimeter-north`
- `perimeter-random`

The game supplies a resolver that maps those channels to perimeter poses. World Spawning only consumes the resolved `SpawnPose` and returns pooled objects.

## Classic Tower Defense

Classic Tower Defense can use channels such as:

- `lane-a-entry`
- `lane-b-entry`

The game supplies a lane-entry resolver. Path following, tower placement, leaks, and target selection remain outside the package.
