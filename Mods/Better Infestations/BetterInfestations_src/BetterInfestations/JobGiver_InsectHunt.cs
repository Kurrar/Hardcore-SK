using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_InsectHunt: ThinkNode_JobGiver
    {
        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            bool killChance = true;
            if (Rand.Range(0f, 1f) > 0.4f)
            {
                killChance = false;
            }
            Job job = new Job(JobDefOf.AttackMelee, target)
            {
                canBash = true,
                maxNumMeleeAttacks = 6,
                expiryInterval = 480,
                attackDoorIfTargetLost = true,
                killIncappedTarget = killChance
            };
            return job;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "AttackMelee"))
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            if (!BetterInfestationsMod.settings.allowHuntingJob)
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
            return MeleeAttackJob(pawn, target);
        }
        public static Job ForceJob(Pawn pawn, Thing target)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "AttackMelee"))
            {
                return null;
            }
            bool killChance = true;
            if (Rand.Range(0f, 1f) > 0.4f)
            {
                killChance = false;
            }
            return new Job(JobDefOf.AttackMelee, target)
            {
                canBash = true,
                maxNumMeleeAttacks = 6,
                expiryInterval = 480,
                attackDoorIfTargetLost = true,
                killIncappedTarget = killChance
            };
        }
        public static Thing FindTarget(Pawn pawn)
        {
            List<Pawn> allPawns = pawn.Map.mapPawns.AllPawnsSpawned;
            if (allPawns == null || !allPawns.Any())
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            List<Thing> targetList = new List<Thing>();
            foreach (Pawn target in allPawns)
            {
                if (target.DestroyedOrNull())
                {
                    continue;
                }
                if (target.Faction != null && target.Faction == pawn.Faction)
                {
                    continue;
                }
                if (target.Downed && JobGiver_InsectGather.WithinHive(pawn, target, true))
                {
                    continue;
                }
                if (!BetterInfestationsMod.settings.disabledDefList.Contains(target.def.defName))
                {
                    targetList.Add(target as Thing);
                }
            }           
            return JobGiver_InsectGather.GetClosest(pawn, targetList);
        }

        public static int MaxDistance(Pawn pawn)
        {
            int maxDistance = 99999;
            int foodAmount = HiveUtility.HiveFoodCount(HiveUtility.FindQueen(pawn));
            if (foodAmount <= 0)
            {
                return maxDistance;
            }
            Insect insect = pawn as Insect;
            if (insect != null)
            {
                if (!insect.worker && !insect.stealFood)
                {
                    return Rand.Range(12, 20);
                }
            }
         
            return maxDistance;
        }
        public static bool CanHunt(Pawn pawn)
        {
            if (pawn.def.defName.Contains("Megascarab"))
            {
                return false;
            }
            return true;
        }
    }
}
