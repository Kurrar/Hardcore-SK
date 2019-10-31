using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
	public class JobGiver_InsectRest : ThinkNode_JobGiver
	{
		private RestCategory minCategory;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
            JobGiver_InsectRest jobGiver_InsectRest = (JobGiver_InsectRest)base.DeepCopy(resolve);
            jobGiver_InsectRest.minCategory = minCategory;
			return jobGiver_InsectRest;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null)
			{
				return 0f;
			}
			if (rest.CurCategory < minCategory)
			{
				return 0f;
			}
			if (Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
			{
				return 0f;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
			{
				return 0f;
			}
			TimeAssignmentDef timeAssignmentDef;
			if (pawn.RaceProps.Humanlike)
			{
				timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
			}
			else
			{
				int num = GenLocalDate.HourOfDay(pawn);
				if (num < 7 || num > 21)
				{
					timeAssignmentDef = TimeAssignmentDefOf.Sleep;
				}
				else
				{
					timeAssignmentDef = TimeAssignmentDefOf.Anything;
				}
			}
			float curLevel = rest.CurLevel;
			if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
			{
				if (curLevel < 0.3f)
				{
					return 8f;
				}
				return 0f;
			}
			else
			{
                if (timeAssignmentDef == TimeAssignmentDefOf.Work)
                {
                    return 0f;
                }
                if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
                {
                    if (curLevel < 0.3f)
                    {
                        return 8f;
                    }
                    return 0f;
                }
                else
				{
					if (timeAssignmentDef != TimeAssignmentDefOf.Sleep)
					{
						throw new NotImplementedException();
					}
					if (curLevel < RestUtility.FallAsleepMaxLevel(pawn))
					{
						return 8f;
					}
					return 0f;
				}
			}
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
            if (HiveUtility.JobGivenRecentTick(pawn, "LayDown"))
            {
                return null;
            }
            Need_Rest rest = pawn.needs.rest;
			if (rest == null || rest.CurCategory < minCategory)
			{
				return null;
			}
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return null;
			}
			return new Job(JobDefOf.LayDown, FindGroundSleepSpotFor(pawn));
		}

		private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
		{
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null)
            {
                IntVec3 pos;
                pos = queen.hiveLocation;
                List<Egg> eggs = queen.spawnedEggs;
                if (eggs != null && eggs.Any())
                {
                    Egg egg = eggs.RandomElement();
                    if (egg != null)
                    {
                        pos = egg.Position;
                    }
                }
                if (!pawn.CanReach(pos, PathEndMode.OnCell, Danger.Deadly, true, TraverseMode.PassDoors))
                {
                    pos = pawn.Position;
                }
                for (int i = 0; i < 2; i++)
                {
                    int radius = (i != 0) ? 12 : 4;
                    IntVec3 result;
                    if (CellFinder.TryRandomClosewalkCellNear(pos, pawn.Map, radius, out result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(pawn.Map).avoidWander))
                    {
                        return result;
                    }
                }
            }
			return CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, pawn.Map, 4, pawn);
		}
	}
}
