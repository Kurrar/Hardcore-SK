using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		private HungerCategory minCategory;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetFood jobGiver_GetFood = (JobGiver_GetFood)base.DeepCopy(resolve);
			jobGiver_GetFood.minCategory = minCategory;
			return jobGiver_GetFood;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null)
			{
				return 0f;
			}
			if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return 0f;
			}
			if (food.CurCategory < minCategory)
			{
				return 0f;
			}
			if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_Ingest"))
            {
                return null;
            }
            Need_Food food = pawn.needs.food;
			if (food == null || food.CurCategory < minCategory)
			{
				return null;
			}
            Predicate<Thing> validator = (Thing t) => t.def.category == ThingCategory.Item && t.IngestibleNow && t.def.defName == "InsectJelly" && pawn.RaceProps.CanEverEat(t) && pawn.CanReserve(t);
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 12f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
            if (thing == null)
            {
                return null;
            }
            float nutrition = FoodUtility.GetNutrition(thing, thing.def);
			return new Job(DefDatabase<JobDef>.GetNamed("BI_Ingest"), thing)
			{
				count = FoodUtility.WillIngestStackCountOf(pawn, thing.def, nutrition)
			};
		}
	}
}
