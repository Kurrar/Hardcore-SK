using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
	public class JobGiver_HiveDefense : JobGiver_AIFightEnemies
	{
        protected override IntVec3 GetFlagPosition(Pawn pawn)
		{
			Queen queen = pawn.mindState.duty.focus.Thing as Queen;
			if (queen != null && queen.Spawned)
			{
				return queen.Position;
			}
			return pawn.Position;
		}

		protected override float GetFlagRadius(Pawn pawn)
		{
			return pawn.mindState.duty.radius;
		}

		protected override Job MeleeAttackJob(Thing enemyTarget)
		{
            Job job = base.MeleeAttackJob(enemyTarget);
			job.attackDoorIfTargetLost = true;
			return job;
        }
	}
}
