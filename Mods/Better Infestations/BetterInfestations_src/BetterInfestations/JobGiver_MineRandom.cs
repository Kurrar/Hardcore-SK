using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;

namespace BetterInfestations
{
	public class JobGiver_MineRandom : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
            if (HiveUtility.JobGivenRecentTick(pawn, "Mine"))
            {
                return null;
            }
            Region region = pawn.GetRegion(RegionType.Set_Passable);
			if (region == null)
			{
				return null;
			}
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen == null)
            {
                return null;
            }
            IntVec3 pos = IntVec3.Invalid;
            List<Egg> eggs = queen.spawnedEggs;
            if (eggs != null && eggs.Any())
            {
                pos = eggs.RandomElement().Position;
            }
            else
            {
                pos = queen.hiveLocation;
            }
            for (int i = 0; i < 40; i++)
            {
                IntVec3 randomCell = region.RandomCell;
                for (int j = 0; j < 4; j++)
                {
                    IntVec3 c = randomCell + GenAdj.CardinalDirections[j];
                    int dist = IntVec3Utility.ManhattanDistanceFlat(c, pos);
                    if (dist > 3)
                    {
                        continue;
                    }
                    if (!c.InBounds(pawn.Map))
                    {
                        continue;
                    }
                    Building edifice = c.GetEdifice(pawn.Map);
                    if (edifice != null && (edifice.def.passability == Traversability.Impassable || edifice.def.IsDoor) && edifice.def.size == IntVec2.One && edifice.def != ThingDefOf.CollapsedRocks && pawn.CanReserve(edifice, 1, -1, null, false))
                    {
                        return new Job(JobDefOf.Mine, edifice)
                        {
                            ignoreDesignations = true
                        };
                    }
                }
            }
			return null;
        }
	}
}
