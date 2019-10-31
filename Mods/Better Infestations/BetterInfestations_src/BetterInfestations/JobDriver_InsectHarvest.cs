using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BetterInfestations
{
	public class JobDriver_InsectHarvest : JobDriver
	{
		private float workDone;
		protected const TargetIndex PlantInd = TargetIndex.A;

		protected Plant Plant
		{
			get
			{
				return (Plant)job.targetA.Thing;
			}
		}

		protected virtual DesignationDef RequiredDesignation
		{
			get
			{
				return null;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			LocalTargetInfo target = job.GetTarget(TargetIndex.A);
			if (target.IsValid)
			{
				Pawn pawn = this.pawn;
				LocalTargetInfo target2 = target;
				Job job = this.job;
				if (!pawn.Reserve(target2, job, 1, -1, null, errorOnFailed))
				{
					return false;
				}
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job, 1, -1, null);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
			Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A, (RequiredDesignation == null) ? null : new Func<Thing, bool>((Thing t) => Map.designationManager.DesignationOn(t, RequiredDesignation) != null));
			yield return initExtractTargetFromQueue;
			yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A, true);
			Toil gotoThing = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue);
			if (RequiredDesignation != null)
			{
				gotoThing.FailOnThingMissingDesignation(TargetIndex.A, RequiredDesignation);
			}
			yield return gotoThing;
			Toil cut = new Toil();
			cut.tickAction = delegate
			{
				Pawn actor = cut.actor;
                Job curJob = actor.jobs.curJob;
                curJob.expiryInterval = -1;
                float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				float num = statValue;
				Plant plant = Plant;
				num *= Mathf.Lerp(3.3f, 1f, plant.Growth);
				workDone += num;
				if (workDone >= plant.def.plant.harvestWork)
				{
                    if (plant.def.plant.harvestedThingDef != null)
					{
						int num2 = plant.YieldNow();
						if (num2 > 0)
						{
							Thing thing = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef, null);
							thing.stackCount = num2;
							if (actor.Faction != Faction.OfPlayer)
							{
								thing.SetForbidden(true, true);
							}
							GenPlace.TryPlaceThing(thing, actor.Position, Map, ThingPlaceMode.Near, null, null);
						}
					}
					plant.def.plant.soundHarvestFinish.PlayOneShot(actor);
					plant.PlantCollected();
					workDone = 0f;
					ReadyForNextToil();
					return;
				}
			};
			cut.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			if (RequiredDesignation != null)
			{
				cut.FailOnThingMissingDesignation(TargetIndex.A, RequiredDesignation);
			}
			cut.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			cut.defaultCompleteMode = ToilCompleteMode.Never;
			cut.WithEffect(EffecterDefOf.Harvest, TargetIndex.A);
			cut.WithProgressBar(TargetIndex.A, () => workDone / Plant.def.plant.harvestWork, true, -0.5f);
			cut.PlaySustainerOrSound(() => Plant.def.plant.soundHarvesting);
			cut.activeSkill = (() => SkillDefOf.Plants);
            yield return cut;
            Toil plantWorkDoneToil = PlantWorkDoneToil();
			if (plantWorkDoneToil != null)
			{
				yield return plantWorkDoneToil;
			}
			yield return Toils_Jump.Jump(initExtractTargetFromQueue);
		}

        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workDone, "workDone", 0f, false);
		}

		protected virtual Toil PlantWorkDoneToil()
		{
			return null;
		}
	}
}