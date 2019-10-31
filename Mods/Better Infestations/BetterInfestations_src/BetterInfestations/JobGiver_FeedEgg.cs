using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_FeedEgg : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_HaulToCell"))
            {
                return null;
            }
            Egg egg = FindEgg(pawn);
            if (egg == null)
            {
                return null;
            }
            Thing target = FindTarget(pawn);
            if (target == null)
            {
                return null;
            }
            CompInsectSpawner comp = egg.TryGetComp<CompInsectSpawner>();
            int jellyCount = comp.jellyMax - comp.jellyStores;
            return new Job(DefDatabase<JobDef>.GetNamed("BI_HaulToCell"), target, egg.Position)
            {
                canBash = true,
                haulOpportunisticDuplicates = false,
                haulMode = HaulMode.ToCellNonStorage,
                expiryInterval = 240,
                ignoreForbidden = true,
                count = jellyCount
            };
        }
        public static Egg FindEgg(Pawn pawn)
        {
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null)
            {
                List<Egg> eggs = queen.spawnedEggs;
                if (eggs != null && eggs.Any())
                {
                    foreach (Egg egg in eggs)
                    {
                        CompInsectSpawner comp = egg.TryGetComp<CompInsectSpawner>();
                        if (comp != null)
                        {
                            if (comp.jellyMax > comp.jellyStores)
                            {
                                if (pawn.CanReserve(egg.Position) && pawn.CanReach(egg.Position, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors))
                                {
                                    return egg;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        public static Thing FindTarget(Pawn pawn)
        {
            List<Thing> allThings = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.InsectJelly);
            if (allThings == null || !allThings.Any())
            {
                return null;
            }
            List<Thing> targetList = new List<Thing>();
            foreach (Thing thing in allThings)
            {
                if (JobGiver_InsectGather.WithinHive(pawn, thing, false))
                {
                    targetList.Add(thing);
                }
            }
            return JobGiver_InsectGather.GetClosest(pawn, targetList);
        }
    }
}
