using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
	public class JobDriver_Ingest : JobDriver
	{
		public const TargetIndex IngestibleSourceInd = TargetIndex.A;

		private Thing IngestibleSource
		{
			get
			{
				return job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private float ChewDurationMultiplier
		{
			get
			{
				Thing ingestibleSource = IngestibleSource;
				if (ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat)
				{
					return 1f;
				}
				return 1f / pawn.GetStatValue(StatDefOf.EatingSpeed, true);
			}
		}

		public override string GetReport()
		{
            Thing thing = job.targetA.Thing;
			if (thing != null && thing.def.ingestible != null)
			{
				if (!thing.def.ingestible.ingestReportStringEat.NullOrEmpty())
				{
					return string.Format(thing.def.ingestible.ingestReportStringEat, job.targetA.Thing.LabelShort);
				}
				if (!thing.def.ingestible.ingestReportString.NullOrEmpty())
				{
					return string.Format(thing.def.ingestible.ingestReportString, job.targetA.Thing.LabelShort);
				}
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Thing ingestibleSource = IngestibleSource;
			int num = FoodUtility.WillIngestStackCountOf(this.pawn, ingestibleSource.def, ingestibleSource.GetStatValue(StatDefOf.Nutrition, true));
			if (num >= ingestibleSource.stackCount && ingestibleSource.Spawned)
			{
				Pawn pawn = this.pawn;
				LocalTargetInfo target = ingestibleSource;
				Job job = this.job;
				if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
				{
					return false;
				}
			}
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !IngestibleSource.Destroyed && !IngestibleSource.IngestibleNow);
			Toil chew = Toils_Ingest.ChewIngestible(pawn, ChewDurationMultiplier, TargetIndex.A, TargetIndex.None).FailOn((Toil x) => !IngestibleSource.Spawned && (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			foreach (Toil toil in PrepareToIngestToils())
			{
				yield return toil;
			}
			yield return chew;
			yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.A);
		}

		[DebuggerHidden]
		private IEnumerable<Toil> PrepareToIngestToils()
		{
			yield return ReserveFoodIfWillIngestWholeStack();
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		}

		private Toil ReserveFoodIfWillIngestWholeStack()
		{
			return new Toil
			{
				initAction = delegate
				{
					if (pawn.Faction == null)
					{
						return;
					}
					Thing thing = job.GetTarget(TargetIndex.A).Thing;
					if (pawn.carryTracker.CarriedThing == thing)
					{
						return;
					}
					int num = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.GetStatValue(StatDefOf.Nutrition, true));
					if (num >= thing.stackCount)
					{
						if (!thing.Spawned)
						{
							pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
							return;
						}
						pawn.Reserve(thing, job, 1, -1, null, true);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
			return ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, pawn);
		}

		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
		{
			if (pawn.pather.Moving)
			{
				return false;
			}
			Thing carriedThing = pawn.carryTracker.CarriedThing;
			if (carriedThing == null || !carriedThing.IngestibleNow)
			{
				return false;
			}
			if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable)
			{
				drawPos = new Vector3(placeCell.x + 0.5f, drawPos.y, placeCell.z + 0.5f);
				return true;
			}
			if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
			{
				HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
				if (holdOffset != null)
				{
					drawPos += holdOffset.offset;
					behind = holdOffset.behind;
					flip = holdOffset.flip;
					return true;
				}
			}
			return false;
		}
	}
}
