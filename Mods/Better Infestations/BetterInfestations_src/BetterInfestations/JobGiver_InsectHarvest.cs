using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_InsectHarvest : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_InsectHarvest"))
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            if (!BetterInfestationsMod.settings.allowHarvestJob)
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
            if (insect == null)
            {
                return null;
            }
            int foodAmount = HiveUtility.HiveFoodCount(queen);
            if (foodAmount >= BetterInfestationsMod.settings.foodStorage && !insect.stealFood)
            {
                return null;
            }
            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }
            Thing target = FindTarget(pawn);
            if (target != null)
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
                if (Rand.Range(1, 25) == 1 && BetterInfestationsMod.settings.allowSapperJob)
                {
                    Thing sapperTarget = JobGiver_InsectSapper.FindTarget(pawn);
                    if (sapperTarget != null && sapperTarget != queen as Thing)
                    {
                        if (JobGiver_InsectGather.FindCloserTarget(pawn, target, sapperTarget))
                        {
                            return JobGiver_InsectSapper.ForceJob(pawn, sapperTarget);
                        }
                    }
                }
            }
            if (target == null)
            {
                return null;
            }
            Job job = new Job(DefDatabase<JobDef>.GetNamed("BI_InsectHarvest"), target)
            {
                canBash = true,
                expiryInterval = 480,
                count = 1
            };
            return job;
        }
        public static Job ForceJob(Pawn pawn, Thing target)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_InsectHarvest"))
            {
                return null;
            }
            return new Job(DefDatabase<JobDef>.GetNamed("BI_InsectHarvest"), target)
            {
                canBash = true,
                expiryInterval = 480,
                count = 1
            };
        }
        public static Thing FindTarget(Pawn pawn)
        {
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            Predicate<Plant> validator = (Plant p) => p.HarvestableNow && p.def.plant.harvestedThingDef != null && (p.def.plant.harvestedThingDef.IsIngestible || p.def.plant.harvestedThingDef.defName == "WoodLog") && pawn.CanReserve(p) && pawn.CanReach(p, PathEndMode.OnCell, Danger.Deadly, true, TraverseMode.PassDoors) && !BetterInfestationsMod.settings.disabledDefList.Contains(p.def.defName);
            Plant plant = HarvestHelper.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Plant), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 250f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
            return plant as Thing;
        }

        public static bool CanHarvest(Pawn pawn)
        {
            if (pawn.def.defName.Contains("Megascarab"))
            {
                return true;
            }
            return false;
        }
    }
}