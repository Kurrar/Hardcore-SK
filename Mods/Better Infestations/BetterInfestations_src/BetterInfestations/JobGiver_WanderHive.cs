using Verse;
using Verse.AI;

namespace BetterInfestations
{
	public class JobGiver_WanderHive : JobGiver_Wander
	{
		public JobGiver_WanderHive()
		{
			wanderRadius = 7.5f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null)
            {
                return queen.Position;
            }
			return pawn.Position;
		}
	}
}
