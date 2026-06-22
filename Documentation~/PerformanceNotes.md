# Performance Notes

The package is designed for warmed pool reuse:

- no direct instantiate per spawn while warmed capacity remains available
- no direct destroy in the spawn/despawn hot flow
- explicit capacity failure instead of hidden growth beyond maximum capacity
- snapshots are diagnostics objects and allocate by design

Editor benchmarks use empty prefabs in Unity EditMode Mono. They do not claim mobile, IL2CPP, Burst, or ECS performance.
