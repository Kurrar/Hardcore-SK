using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
	internal class JobGiver_InsectFightFire : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
            if (HiveUtility.JobGivenRecentTick(pawn, "BeatFire"))
            {
                return null;
            }
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn2 = ((AttachableThing)t).parent as Pawn;
                return pawn2 == null && pawn.CanReserve(t, 1, -1, null, false);
            };
            if (JobGiver_InsectGather.WithinHive(pawn, pawn, false))
            {
                Queen queen = HiveUtility.FindQueen(pawn);
                if (queen != null)
                {
                    Thing thingOnFire = GenClosest.ClosestThingReachable(queen.hiveLocation, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 12f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                    if (thingOnFire != null)
                    {
                        return new Job(JobDefOf.BeatFire, thingOnFire);
                    }
                    if (queen.spawnedEggs.Any())
                    {
                        foreach(Egg egg in queen.spawnedEggs)
                        {
                            thingOnFire = GenClosest.ClosestThingReachable(egg.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 5f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                            if (thingOnFire != null)
                            {
                                return new Job(JobDefOf.BeatFire, thingOnFire);
                            }
                        }
                    }
                }
            }
            return null;
        }
	}
}
