# World Spawning Sandbox

The sample demonstrates the intended shape:

1. Encounters emits spawn requests.
2. A channel resolver maps channels to poses.
3. `WorldSpawnService` turns requests into pooled GameObjects.
4. External systems decide when to despawn.

The sample intentionally does not add combat, movement, progression, persistence, UI, path following, or product rules.
