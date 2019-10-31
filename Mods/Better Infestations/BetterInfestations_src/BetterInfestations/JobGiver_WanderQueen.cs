using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_WanderQueen: JobGiver_Wander
    {
        public JobGiver_WanderQueen()
        {
            wanderRadius = 12f;
            ticksBetweenWandersRange = new IntRange(125, 200);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null)
            {
                List<Egg> eggs = queen.spawnedEggs;
                if (eggs != null && eggs.Any())
                {
                    return eggs.RandomElement().Position;
                }
                else
                {
                    return queen.hiveLocation;
                }
            }
            return pawn.Position;
        }
    }
}
