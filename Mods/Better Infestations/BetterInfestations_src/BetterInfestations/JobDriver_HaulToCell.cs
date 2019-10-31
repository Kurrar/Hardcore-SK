using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Diagnostics;

namespace BetterInfestations
{
	public class JobDriver_HaulToCell : JobDriver
	{
		private bool forbiddenInitially;
		private const TargetIndex HaulableInd = TargetIndex.A;
		private const TargetIndex StoreCellInd = TargetIndex.B;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forbiddenInitially, "forbiddenInitially", false, false);
		}

        public override string GetReport()
		{
			IntVec3 cell = job.targetB.Cell;
			Thing thing = null;
			if (pawn.CurJob == job && pawn.carryTracker.CarriedThing != null)
			{
				thing = pawn.carryTracker.CarriedThing;
			}
			else if (TargetThingA != null && TargetThingA.Spawned)
			{
				thing = TargetThingA;
			}
			if (thing == null)
			{
				return "ReportHaulingUnknown".Translate();
			}
			string text = null;
			SlotGroup slotGroup = cell.GetSlotGroup(Map);
			if (slotGroup != null)
			{
				text = slotGroup.parent.SlotYielderLabel();
			}
			if (text != null)
			{
				return "ReportHaulingTo".Translate(thing.Label, text.Named("DESTINATION"), thing.Named("THING"));
			}
			return "ReportHauling".Translate(thing.Label, thing);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.job.GetTarget(TargetIndex.B);
			Job job = this.job;
			bool result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				pawn = this.pawn;
				target = this.job.GetTarget(TargetIndex.A);
				job = this.job;
				result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
			}
			else
			{
                result = false;
			}
			return result;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			if (TargetThingA != null)
			{
				forbiddenInitially = TargetThingA.IsForbidden(pawn);
			}
			else
			{
				forbiddenInitially = false;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.B);
			if (!forbiddenInitially)
			{
				this.FailOnForbidden(TargetIndex.A);
			}
			Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return reserveTargetA;
			Toil toilGoto = null;
            toilGoto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            toilGoto.AddFailCondition(delegate
            {
                Pawn actor = toilGoto.actor;
                Job curJob = actor.jobs.curJob;
                if (curJob.haulMode == HaulMode.ToCellStorage)
                {
                    Thing thing = curJob.GetTarget(TargetIndex.A).Thing;
                    IntVec3 cell = actor.jobs.curJob.GetTarget(TargetIndex.B).Cell;
                    if (!cell.IsValidStorageFor(Map, thing))
                    {
                        return true;
                    }
                }
                return false;
            });
            yield return toilGoto;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
			if (job.haulOpportunisticDuplicates)
			{
				yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveTargetA, TargetIndex.A, TargetIndex.B, false, null);
			}
            if (job.expiryInterval != -1)
            {
                yield return CheckExpiry();
            }
            Insect insect = toilGoto.actor as Insect;
            if (insect != null && insect.targetColonyFood)
            {
                yield return ResetTargetColonyFoodFlag(insect);
            }
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
            if (insect != null && insect.stealFood)
            {
                yield return ResetStealFoodFlag(insect);
            }
        }
        public static Toil CheckExpiry()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                if (actor.carryTracker != null && actor.carryTracker.CarriedThing != null)
                {
                    curJob.expiryInterval = -1;
                }
            };
            return toil;
        }
        public static Toil ResetTargetColonyFoodFlag(Insect insect)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                insect.targetColonyFood = false;
            };
            return toil;
        }
        public static Toil ResetStealFoodFlag(Insect insect)
        {
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                insect.stealFood = false;
                Queen queen = HiveUtility.FindQueen(insect);
                if (queen != null && !queen.colonyFoodFound)
                {
                    queen.colonyFoodFound = true;
                    if (BetterInfestationsMod.settings.showNotifications)
                    {
                        Find.LetterStack.ReceiveLetter("letterlabelfoodinfestationsiege".Translate(), "lettertextfoodinfestationsiege".Translate(), LetterDefOf.ThreatBig, queen);
                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                    }
                }
            };
            return toil;
        }
    }
}
