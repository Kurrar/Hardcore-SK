using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_LayEgg: ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_LayEgg"))
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            Queen queen = pawn as Queen;
            if (queen == null)
            {
                return null;
            }
            if (!queen.eggReady)
            {
                return null;
            }
            if (queen.hiveLocation == IntVec3.Invalid || HiveUtility.TotalHiveInsectCount(queen) >= BetterInfestationsMod.settings.maxInsectsHive || HiveUtility.TotalSpawnedInsectCount(queen.Map) >= BetterInfestationsMod.settings.maxInsectsMap)
            {
                return null;
            }
            Map map = pawn.Map;
            Predicate<IntVec3> baseValidator = delegate (IntVec3 cell)
            {
                if (!cell.Standable(map) || !cell.Walkable(map) || cell.GetFirstItem(map) != null || cell.GetFirstBuilding(map) != null || cell.GetFirstPawn(map) != null || !pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.NoPassClosedDoors))
                {
                    return false;
                }
                return true;
            };
            IntVec3 loc;
            if (CellFinder.TryFindRandomCellNear(queen.hiveLocation, map, 12, (IntVec3 cell) => baseValidator(cell) && cell.Roofed(map), out loc, -1))
            {
                return new Job(DefDatabase<JobDef>.GetNamed("BI_LayEgg"), loc);
            }
            if (CellFinder.TryFindRandomCellNear(queen.hiveLocation, map, 12, (IntVec3 cell) => baseValidator(cell), out loc, 3))
            {
                return new Job(DefDatabase<JobDef>.GetNamed("BI_LayEgg"), loc);
            }
            return null;
        }
    }
}
