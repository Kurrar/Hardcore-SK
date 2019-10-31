using Verse;

namespace BetterInfestations
{
	public class CompProperties_FilthSpawner : CompProperties
	{
		public ThingDef filthDef;
        public int spawnCountOnSpawn = 5;
        public float spawnRadius = 2f;

        public CompProperties_FilthSpawner()
		{
			compClass = typeof(CompFilthSpawner);
		}
	}
}
