# Package Validation

Validation project:

`C:/Repositories/Deucarian/WorldSpawning-TestProject`

Unity version:

`6000.3.5f1`

Local dependencies:

- `com.deucarian.gameplay-foundation`
- `com.deucarian.encounters`
- `com.deucarian.world-spawning`

## Results

Import:

```text
Unity.exe -batchmode -quit -projectPath C:/Repositories/Deucarian/WorldSpawning-TestProject -logFile C:/Repositories/Deucarian/WorldSpawning-TestProject-import-1.log
```

Result: passed, return code 0, no compiler errors.

EditMode:

```text
Unity.exe -batchmode -projectPath C:/Repositories/Deucarian/WorldSpawning-TestProject -executeMethod BatchTestRunner.RunEditMode -batchTestResults C:/Repositories/Deucarian/WorldSpawning-TestProject-edit-2.txt -logFile C:/Repositories/Deucarian/WorldSpawning-TestProject-edit-2.log
```

Result: `result=Passed; passCount=9; failCount=0; skipCount=0; duration=0,579`

EditMode repeat: `result=Passed; passCount=9; failCount=0; skipCount=0; duration=0,531`

PlayMode:

```text
Unity.exe -batchmode -projectPath C:/Repositories/Deucarian/WorldSpawning-TestProject -runTests -testPlatform PlayMode -testResults C:/Repositories/Deucarian/WorldSpawning-TestProject-play-results.xml -logFile C:/Repositories/Deucarian/WorldSpawning-TestProject-play-xml.log
```

Result: XML `test-run` passed, `total=1`, `passed=1`, `failed=0`, `duration=0,0576232`.

PlayMode repeat: XML `test-run` passed, `total=1`, `passed=1`, `failed=0`, `duration=0,0566141`.

The custom `BatchTestRunner.RunPlayMode` entered PlayMode but did not emit its compact result file after play-mode domain reload; direct Unity `-runTests -testPlatform PlayMode` was used for PlayMode validation.

## Benchmark

Benchmark output is written to:

`C:/Repositories/Deucarian/WorldSpawning-TestProject/Logs/world-spawning-benchmark-results.json`

- 1,000 pooled spawn/despawn cycles: 18.507 ms, 0 bytes allocated
- 5,000 pooled spawn/despawn cycles: 79.914 ms, 0 bytes allocated
- 10,000 pooled spawn/despawn cycles: 189.608 ms, 0 bytes allocated

Pool warmup setting: initial capacity equals operation count; maximum capacity equals operation count.

Prefab complexity: single empty GameObject.

These are Unity EditMode Mono results, not mobile, IL2CPP, Burst, or ECS results.
