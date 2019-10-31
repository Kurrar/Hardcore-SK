using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_InsectGather : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_HaulToCell"))
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
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
            Thing target = FindTarget(pawn, false);
            if (target != null)
            {
                if (JobGiver_InsectHunt.CanHunt(pawn) && BetterInfestationsMod.settings.allowHuntingJob)
                {
                    Thing huntTarget = JobGiver_InsectHunt.FindTarget(pawn);
                    if (huntTarget != null)
                    {
                        if (FindCloserTarget(pawn, target, huntTarget))
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
                        if (FindCloserTarget(pawn, target, harvestTarget))
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
                        if (FindCloserTarget(pawn, target, sapperTarget))
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
            IntVec3 cell = FindCell(pawn);
            if (cell == IntVec3.Invalid)
            {
                return null;
            }
            return new Job(DefDatabase<JobDef>.GetNamed("BI_HaulToCell"), target, cell)
            {
                canBash = true,
                haulOpportunisticDuplicates = false,
                haulMode = HaulMode.ToCellNonStorage,
                expiryInterval = 480,
                ignoreForbidden = true,
                count = 99999
            };
        }
        public static Job ForceJob(Pawn pawn, Thing target)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_HaulToCell"))
            {
                return null;
            }
            IntVec3 cell = FindCell(pawn);
            if (cell == IntVec3.Invalid)
            {
                return null;
            }
            return new Job(DefDatabase<JobDef>.GetNamed("BI_HaulToCell"), target, cell)
            {
                canBash = true,
                haulOpportunisticDuplicates = false,
                haulMode = HaulMode.ToCellNonStorage,
                expiryInterval = 480,
                ignoreForbidden = true,
                count = 99999
            };
        }
        public static IntVec3 FindCell(Pawn pawn)
        {
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null)
            {
                IntVec3 cell = IntVec3.Invalid;
                List<Egg> eggs = queen.spawnedEggs;
                if (eggs != null && eggs.Any())
                {
                    cell = eggs.RandomElement().Position;
                }
                else
                {
                    cell = queen.hiveLocation;
                }
                IntVec3 pos = IntVec3.Invalid;
                pos = CellFinder.RandomClosewalkCellNear(cell, pawn.Map, 5);
                if (pos != IntVec3.Invalid && pawn.CanReserve(pos) && pawn.CanReach(pos, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors))
                {
                    return pos;
                }
            }
            return IntVec3.Invalid;
        }
        public static bool FindCloserTarget(Pawn pawn, Thing targetA, Thing targetB)
        {
            IntVec3 pos = pawn.Position;
            Insect insect = pawn as Insect;
            if (insect != null && insect.targetColonyFood)
            {
                Queen queen = HiveUtility.FindQueen(pawn);
                if (queen != null)
                {
                    pos = queen.colonyFoodLoc;
                }
            }
            float dist = IntVec3Utility.ManhattanDistanceFlat(targetA.Position, pos);
            float dist2 = IntVec3Utility.ManhattanDistanceFlat(targetB.Position, pos);
            if (dist2 < dist)
            {
                return true;
            }
            return false;
        }
        public static Thing FindTarget(Pawn pawn, bool butcherJob)
        {
            List<Thing> allThings = pawn.Map.listerThings.AllThings;
            if (allThings == null || !allThings.Any())
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            List<Thing> targetList = new List<Thing>();
            List<Thing> allyList = new List<Thing>();
            foreach (Thing thing in allThings)
            {
                Corpse corpse = thing as Corpse;
                Pawn p = thing as Pawn;
                if (thing != null && thing.def.category == ThingCategory.Item && !thing.def.IsCorpse && (thing.IngestibleNow || thing.def.defName == "WoodLog"))
                {
                    if (!butcherJob && !WithinHive(pawn, thing, true) || (butcherJob && WithinHive(pawn, thing, false) && thing.def.defName != "InsectJelly"))
                    {
                        if (!BetterInfestationsMod.settings.disabledDefList.Contains(thing.def.defName))
                        {
                            targetList.Add(thing);
                        }
                    }
                }
                else if (corpse != null && corpse.InnerPawn != null && corpse.InnerPawn.RaceProps.IsFlesh && corpse.GetRotStage() != RotStage.Dessicated)
                {
                    if (corpse.InnerPawn.Faction != null && corpse.InnerPawn.Faction == pawn.Faction)
                    {
                        if (butcherJob && WithinHive(pawn, corpse, false))
                        {
                            targetList.Add(thing);
                        }
                        else if (!butcherJob && !WithinHive(pawn, corpse, true))
                        {
                            allyList.Add(thing);
                        }
                    }
                    else if (!butcherJob && !WithinHive(pawn, corpse, true) || butcherJob && WithinHive(pawn, corpse, false))
                    {
                        if (!BetterInfestationsMod.settings.disabledDefList.Contains(corpse.InnerPawn.def.defName))
                        {
                            targetList.Add(thing);
                        }
                    }
                }
                else if (p != null && p.Downed && p.RaceProps.IsFlesh)
                {
                    if (!butcherJob && !WithinHive(pawn, p, true))
                    {
                        if (p.Faction == pawn.Faction)
                        {
                            allyList.Add(thing);
                        }
                        else if (!BetterInfestationsMod.settings.disabledDefList.Contains(p.def.defName))
                        {
                            targetList.Add(thing);
                        }
                    }
                }
            }
            Thing result = GetClosest(pawn, targetList);
            if (result == null)
            {
                result = GetClosest(pawn, allyList);
            }
            return result;
        }
        public static Thing GetClosest(Pawn pawn, List<Thing> things)
        {
            if (things == null || !things.Any())
            {
                return null;
            }

            Thing result = null;
            int best = int.MaxValue;

            IntVec3 pos = pawn.Position;
            Insect insect = pawn as Insect;
            if (insect != null && insect.targetColonyFood)
            {
                Queen queen = HiveUtility.FindQueen(pawn);
                if (queen != null)
                {
                    pos = queen.colonyFoodLoc;
                }
            }

            foreach (Thing thing in things)
            {
                int dist = IntVec3Utility.ManhattanDistanceFlat(pos, thing.Position);
                if (dist < best && dist <= JobGiver_InsectHunt.MaxDistance(pawn))
                {
                    if (pawn.CanReserve(thing) && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors))
                    {
                        best = dist;
                        result = thing;
                    }
                }
            }
            return result;
        }

        public static bool WithinHive(Pawn pawn, Thing thing, bool NearAllyEggs)
        {
            List<Thing> things = HiveUtility.ListQueens(pawn.Map);
            if (things == null || !things.Any())
            {
                return false;
            }
            foreach (Thing t in things)
            {
                Queen queen = t as Queen;
                if (queen != null)
                {
                    if (queen == HiveUtility.FindQueen(pawn) || (NearAllyEggs && queen.Faction == pawn.Faction))
                    {
                        int dist = IntVec3Utility.ManhattanDistanceFlat(queen.hiveLocation, thing.Position);
                        if (dist <= 12)
                        {
                            return true;
                        }
                        List<Egg> eggs = queen.spawnedEggs;
                        if (eggs != null && eggs.Any())
                        {
                            foreach (Egg egg in eggs)
                            {
                                if (egg != null)
                                {
                                    dist = IntVec3Utility.ManhattanDistanceFlat(egg.Position, thing.Position);
                                    if (dist <= 5)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
