using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;

namespace BetterInfestations
{
	public class JobGiver_MaintainEggs : ThinkNode_JobGiver
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_Maintain"))
            {
                return null;
            }
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null && JobGiver_InsectGather.WithinHive(pawn, pawn, true))
            {
                List<Egg> eggs = queen.spawnedEggs;
                if (eggs != null && eggs.Any())
                {
                    foreach (Egg egg in eggs)
                    {
                        if (egg != null && pawn.CanReserve(egg))
                        {
                            CompMaintainable compMaintainable = egg.TryGetComp<CompMaintainable>();
                            if (compMaintainable != null && compMaintainable.CurStage != MaintainableStage.Healthy)
                            {
                                return new Job(DefDatabase<JobDef>.GetNamed("BI_Maintain"), egg);
                            }
                        }
                    }
                }
            }
			return null;
		}
	}
}
