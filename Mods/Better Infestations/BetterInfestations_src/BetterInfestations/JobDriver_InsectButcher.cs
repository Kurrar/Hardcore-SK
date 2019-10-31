using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace BetterInfestations
{
	class JobDriver_InsectButcher : JobDriver
	{
        private Thing thing
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
                Corpse corpse = thing as Corpse;
                if (thing != null && corpse == null)
                {
                    return 4f / pawn.GetStatValue(StatDefOf.EatingSpeed, true);
                }
                return 1f / pawn.GetStatValue(StatDefOf.EatingSpeed, true);
            }
        }

        public override string GetReport()
        {
            if (!job.GetTarget(TargetIndex.A).Thing.def.IsCorpse)
            {
                string txt = "butcher_converting_desc".Translate();
                return ReportStringProcessed(txt).CapitalizeFirst();
            }
            return base.GetReport();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (thing.Spawned && !pawn.Reserve(thing, job, 1, -1, null))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            Toil chew = ChewCorpse(pawn, ChewDurationMultiplier, TargetIndex.A).FailOn((Toil x) => !thing.Spawned).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return chew;
            yield return FinalizeToil();
        }

        public static Toil ChewCorpse(Pawn chewer, float durationMultiplier, TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt(500 * durationMultiplier);
                if (thing.Spawned)
                {
                    thing.Map.physicalInteractionReservationManager.Reserve(chewer, actor.CurJob, thing);
                }
            };
            toil.tickAction = delegate
            {
                if (chewer != toil.actor)
                {
                    toil.actor.rotationTracker.FaceCell(chewer.Position);
                }
                else
                {
                    Thing thing = toil.actor.CurJob.GetTarget(ind).Thing;
                    if (thing != null && thing.Spawned)
                    {
                        toil.actor.rotationTracker.FaceCell(thing.Position);
                    }
                }
            };
            toil.WithProgressBar(ind, delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                if (thing == null)
                {
                    return 1f;
                }
                return 1f - toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round(500 * durationMultiplier);
            }, false, -0.5f);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(ind);
            toil.AddFinishAction(delegate
            {
                if (chewer == null)
                {
                    return;
                }
                if (chewer.CurJob == null)
                {
                    return;
                }
                Thing thing = chewer.CurJob.GetTarget(ind).Thing;
                if (thing == null)
                {
                    return;
                }
                if (chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
                {
                    chewer.Map.physicalInteractionReservationManager.Release(chewer, toil.actor.CurJob, thing);
                }
            });
            toil.handlingFacing = true;
            AddChewEffects(toil, chewer, ind);
            return toil;
        }
        public static Toil AddChewEffects(Toil toil, Pawn chewer, TargetIndex ind)
        {
            toil.WithEffect(delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                EffecterDef result;
                if (thing.def.ingestible != null)
                {
                    result = thing.def.ingestible.ingestEffect;
                }
                else
                {
                    result = DefDatabase<EffecterDef>.GetNamed("EatVegetarian", true);
                }
                return result;
            }, delegate
            {
                if (!toil.actor.CurJob.GetTarget(ind).HasThing)
                {
                    return null;
                }
                Thing thing = toil.actor.CurJob.GetTarget(ind).Thing;
                if (chewer != toil.actor)
                {
                    return chewer;
                }
                return thing;
            });
            toil.PlaySustainerOrSound(delegate
            {
                LocalTargetInfo target = toil.actor.CurJob.GetTarget(ind);
                if (!target.HasThing)
                {
                    return null;
                }
                SoundDef result = DefDatabase<SoundDef>.GetNamed("RawMeat_Eat", true);
                return result;
            });
            return toil;
        }

        public Toil FinalizeToil()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                ConvertToJelly();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public void ConvertToJelly()
        {
            if (thing != null)
            {
                Corpse corpse = thing as Corpse;
                if (corpse != null)
                {
                    ButcherProducts(corpse);
                    if (!corpse.Destroyed)
                    {
                        corpse.Destroy(DestroyMode.Vanish);
                    }
                }
                else
                {
                    Thing thing2 = ThingMaker.MakeThing(ThingDefOf.InsectJelly, null);
                    thing2.stackCount = thing.stackCount;
                    GenPlace.TryPlaceThing(thing2, thing.Position, thing.Map, ThingPlaceMode.Direct, out Thing jelly, null);
                    if (jelly != null)
                    {
                        jelly.SetForbidden(true);
                    }
                    if (!thing.Destroyed)
                    {
                        thing.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
        public void ButcherProducts(Corpse corpse)
        {
            if (corpse.InnerPawn != null)
            {
                if (corpse.InnerPawn.RaceProps.Humanlike)
                {
                    IntVec3 pos = corpse.PositionHeld;
                    if (corpse.InnerPawn.equipment != null)
                    {
                        corpse.InnerPawn.equipment.DropAllEquipment(pos, true);
                    }
                    if (corpse.InnerPawn.apparel != null)
                    {
                        corpse.InnerPawn.apparel.DropAll(pos, true);
                    }
                    if (corpse.InnerPawn.inventory != null)
                    {
                        corpse.InnerPawn.inventory.DropAllNearPawn(pos, true);
                    }
                }
                if (corpse.InnerPawn.RaceProps.meatDef != null)
                {
                    int count = GenMath.RoundRandom(corpse.InnerPawn.GetStatValue(StatDefOf.MeatAmount, true));
                    if (count > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(corpse.InnerPawn.RaceProps.meatDef, null);
                        thing.stackCount = count;
                        if (thing.stackCount >= 2)
                        {
                            RotStage rotStage = corpse.GetRotStage();
                            if (rotStage == RotStage.Rotting)
                            {
                                thing.stackCount = count / 2;
                            }
                        }
                        GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Direct, out Thing meat, null);
                        if (meat != null)
                        {
                            meat.SetForbidden(true);
                        }
                    }
                }
                if (corpse.InnerPawn.RaceProps.leatherDef != null)
                {
                    int count = GenMath.RoundRandom(corpse.InnerPawn.GetStatValue(StatDefOf.LeatherAmount, true));
                    if (count > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(corpse.InnerPawn.RaceProps.leatherDef, null);
                        thing.stackCount = count;
                        if (thing.stackCount >= 2)
                        {
                            RotStage rotStage = corpse.GetRotStage();
                            if (rotStage == RotStage.Rotting)
                            {
                                thing.stackCount = count / 2;
                            }
                        }
                        GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Direct, out Thing leather, null);
                        if (leather != null)
                        {
                            leather.SetForbidden(true);
                        }
                    }
                }
                if (corpse.InnerPawn.RaceProps.BloodDef != null)
                {
                    FilthMaker.MakeFilth(corpse.Position, corpse.Map, corpse.InnerPawn.RaceProps.BloodDef, corpse.InnerPawn.LabelIndefinite(), 1);
                }
            }
        }
    }
}
