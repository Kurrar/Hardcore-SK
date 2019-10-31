using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_InsectSapper: ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "Mine"))
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            if (!BetterInfestationsMod.settings.allowSapperJob)
            {
                return null;
            }
            if (pawn.GetRoom() != null && pawn.GetRoom().Fogged)
            {
                return null;
            }
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen == null)
            {
                return null;
            }
            Insect insect = pawn as Insect;
            if (insect == null && queen != null && queen.spawnedInsects != null && queen.spawnedInsects.Count > 1)
            {
                return null;
            }
            int foodAmount = HiveUtility.HiveFoodCount(queen);
            if (foodAmount >= BetterInfestationsMod.settings.foodStorage && insect != null && !insect.stealFood)
            {
                return null;
            }
            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }
            Thing target = FindTarget(pawn);
            if (target != null && target != queen as Thing)
            {
                Thing gatherTarget = JobGiver_InsectGather.FindTarget(pawn, false);
                if (gatherTarget != null)
                {
                    if (JobGiver_InsectGather.FindCloserTarget(pawn, target, gatherTarget))
                    {
                        return JobGiver_InsectGather.ForceJob(pawn, gatherTarget);
                    }
                }
                if (JobGiver_InsectHunt.CanHunt(pawn) && BetterInfestationsMod.settings.allowHuntingJob)
                {
                    Thing huntTarget = JobGiver_InsectHunt.FindTarget(pawn);
                    if (huntTarget != null)
                    {
                        if (JobGiver_InsectGather.FindCloserTarget(pawn, target, huntTarget))
                        {
                            return JobGiver_InsectHunt.ForceJob(pawn, huntTarget);
                        }
                    }
                }
                if (JobGiver_InsectHarvest.CanHarvest(pawn) && BetterInfestationsMod.settings.allowHarvestJob)
                {
                    Thing harvestTarget = JobGiver_InsectHarvest.FindTarget(pawn);
                    if (harvestTarget != null)
                    {
                        if (JobGiver_InsectGather.FindCloserTarget(pawn, target, harvestTarget))
                        {
                            return JobGiver_InsectHarvest.ForceJob(pawn, harvestTarget);
                        }
                    }
                }
            }
            if (target == null)
            {
                return null;
            }
            if (!pawn.CanReach(target, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings))
            {
                return null;
            }
            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
            {
                List<IntVec3> cells = pawnPath.NodesReversed;
                if (cells != null && cells.Any())
                {
                    foreach (IntVec3 cell in cells)
                    {
                        Building b = cell.GetEdifice(pawn.Map);
                        if (b != null && b.def != null)
                        {
                            if (b.def.passability != Traversability.Impassable)
                            {
                                return null;
                            }
                            if (b.def.size != IntVec2.One)
                            {
                                return null;
                            }
                            if (b.Faction == null || (b.Faction != null && !b.Faction.IsPlayer))
                            {
                                return null;
                            }
                        }
                    }
                }
                IntVec3 cellBeforeBlocker;
                Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                if (thing != null && pawn.CanReserve(thing) && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors))
                {
                    return new Job(JobDefOf.Mine, thing)
                    {
                        ignoreDesignations = true,
                        expiryInterval = 6000
                    };
                }
            }
            return null;
        }

        public static Job ForceJob(Pawn pawn, Thing target)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "Mine"))
            {
                return null;
            }
            if (!pawn.CanReach(target, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings))
            {
                return null;
            }
            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
            {
                List<IntVec3> cells = pawnPath.NodesReversed;
                if (cells != null && cells.Any())
                {
                    foreach (IntVec3 cell in cells)
                    {
                        Building b = cell.GetEdifice(pawn.Map);
                        if (b != null && b.def != null)
                        {
                            if (b.def.passability != Traversability.Impassable)
                            {
                                return null;
                            }
                            if (b.def.size != IntVec2.One)
                            {
                                return null;
                            }
                            if (b.Faction == null || (b.Faction != null && !b.Faction.IsPlayer))
                            {
                                return null;
                            }
                        }
                    }
                }
                IntVec3 cellBeforeBlocker;
                Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                if (thing != null && pawn.CanReserve(thing) && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors))
                {
                    return new Job(JobDefOf.Mine, thing)
                    {
                        ignoreDesignations = true,
                        expiryInterval = 6000
                    };
                }
            }
            return null;
        }

        public static Thing FindTarget(Pawn pawn)
        {
            List<Thing> allThings = pawn.Map.listerThings.AllThings;
            if (allThings == null || !allThings.Any())
            {
                return null;
            }
            List<Thing> targetList = new List<Thing>();
            foreach (Thing thing in allThings)
            {
                if (thing != null)
                {
                    Corpse corpse = thing as Corpse;
                    Pawn p = thing as Pawn;
                    Queen q = thing as Queen;

                    if (thing.def.category == ThingCategory.Item && !thing.def.IsCorpse && (thing.IngestibleNow || thing.def.defName == "WoodLog") && !JobGiver_InsectGather.WithinHive(pawn, thing, true))
                    {
                        targetList.Add(thing);
                    }
                    else if (corpse != null && corpse.InnerPawn != null && corpse.InnerPawn.RaceProps.IsFlesh && corpse.GetRotStage() != RotStage.Dessicated && !JobGiver_InsectGather.WithinHive(pawn, thing, true))
                    {
                        targetList.Add(thing);
                    }
                    else if (p != null && p.RaceProps.IsFlesh && (p.Faction == null || (p.Faction != null && p.Faction != pawn.Faction)) && !JobGiver_InsectGather.WithinHive(pawn, thing, true))
                    {
                        targetList.Add(thing);
                    }
                    else if (q != null && HiveUtility.FindQueen(pawn) == q)
                    {
                        targetList.Add(thing);
                    }
                }
            }
            return GetClosest(pawn, targetList);
        }
        public static Thing GetClosest(Pawn pawn, List<Thing> list)
        {
            Thing result = null;
            int best = int.MaxValue;

            if (list != null && list.Any())
            {
                Queen queen = HiveUtility.FindQueen(pawn);
                if (queen == null)
                {
                    return null;
                }
                if (list.Contains(queen as Thing) && !pawn.CanReach(queen, PathEndMode.OnCell, Danger.Deadly, true, TraverseMode.PassDoors))
                {
                    return queen;
                }
                else
                {
                    IntVec3 pos = pawn.Position;
                    Insect insect = pawn as Insect;
                    if (insect != null && insect.targetColonyFood)
                    {
                        pos = queen.colonyFoodLoc;
                    }

                    foreach (Thing thing in list)
                    {
                        int dist = IntVec3Utility.ManhattanDistanceFlat(pos, thing.Position);
                        if (dist < best && dist <= JobGiver_InsectHunt.MaxDistance(pawn))
                        {
                            if (!pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly, true, TraverseMode.PassDoors))
                            {
                                best = dist;
                                result = thing;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
