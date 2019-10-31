using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_QueenAttack: ThinkNode_JobGiver
    {
        public static int ticksInAttackMode = 2500;
        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            Job job = new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 6,
                expiryInterval = 480,
                attackDoorIfTargetLost = true,
                killIncappedTarget = false,
            };
            return job;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "AttackMelee"))
            {
                return null;
            }
            Thing target = FindTarget(pawn);
            if (target == null)
            {
                return null;
            }
            return MeleeAttackJob(pawn, target);
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
            List<Thing> targets = new List<Thing>();
            foreach (Pawn target in allPawns)
            {
                if (!target.DestroyedOrNull() && !target.Downed)
                {
                    if (target.Faction == null || (target.Faction != null && target.Faction != pawn.Faction))
                    {
                        bool recentAttack = target.mindState != null && target.mindState.lastAttackedTarget == pawn && target.mindState.lastAttackTargetTick <= Find.TickManager.TicksGame && target.mindState.lastAttackTargetTick > Find.TickManager.TicksGame - ticksInAttackMode;
                        bool withinHiveAndInVisual = JobGiver_InsectGather.WithinHive(pawn, target as Thing, false) && GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map, true, null, 0, 0);
                        if (recentAttack || withinHiveAndInVisual)
                        {
                            if (!BetterInfestationsMod.settings.disabledDefList.Contains(target.def.defName))
                            {
                                targets.Add(target as Thing);
                            }
                        }
                    }
                }
            }
            return JobGiver_InsectGather.GetClosest(pawn, targets);
        }
    }
}
