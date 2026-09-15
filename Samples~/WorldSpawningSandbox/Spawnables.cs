namespace Deucarian.WorldSpawning.Samples.SimpleUsage
{
    [SpawnableKeySet]
    public static class Spawnables
    {
        public static SpawnableKey Goblin => new Definition();
        private sealed class Definition : SpawnableKey
        {
            public Definition() : base("enemy.goblin") { }
        }
    }
}
