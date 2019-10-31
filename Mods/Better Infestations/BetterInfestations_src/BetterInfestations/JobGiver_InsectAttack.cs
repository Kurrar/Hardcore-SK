using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_InsectAttack: ThinkNode_JobGiver
    {
        public static int maxAttackDistance = 130;
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
            List<Thing> allThings = pawn.Map.listerThings.AllThings;
            if (allThings == null || !allThings.Any())
            {
                return null;
            }
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            List<Thing> targets = new List<Thing>();
            foreach (Thing target in allThings)
            {
                Pawn pawnTarget = target as Pawn;
                Building_Turret turretTarget = target as Building_Turret;
                Egg eggTarget = target as Egg;
                if (!pawnTarget.DestroyedOrNull() && !pawnTarget.Downed)
                {
                    if ((pawnTarget.Faction != null && pawnTarget.Faction != pawn.Faction) || pawnTarget.Faction == null)
                    {
                        if (!BetterInfestationsMod.settings.disabledDefList.Contains(pawnTarget.def.defName))
                        {
                            targets.Add(target);
                        }
                    }
                }
                else if (!turretTarget.DestroyedOrNull())
                {
                    if ((turretTarget.Faction != null && turretTarget.Faction != pawn.Faction) || turretTarget.Faction == null)
                    {
                        targets.Add(target);
                    }
                }
                else if (!eggTarget.DestroyedOrNull())
                {
                    if ((eggTarget.Faction != null && eggTarget.Faction != pawn.Faction) || eggTarget.Faction == null)
                    {
                        targets.Add(target);
                    }
                }
            }
            Thing result = null;
            if (pawn.mindState.enemyTarget == null)
            {
                if (targets.Any())
                {
                    foreach (Thing target in targets)
                    {
                        Pawn pawnTarget = target as Pawn;
                        Building_Turret turretTarget = target as Building_Turret;
                        Egg eggTarget = target as Egg;
                        bool attackingHive;
                        if (pawnTarget != null)
                        {
                            attackingHive = pawnTarget.mindState.lastAttackedTarget == pawn;
                            result = GetAttacker(pawn, target, attackingHive);
                        }
                        else if (turretTarget != null)
                        {
                            attackingHive = turretTarget.LastAttackedTarget == pawn;
                            result = GetAttacker(pawn, target, attackingHive);
                        }
                        else if (eggTarget != null && GenSight.LineOfSight(pawn.Position, eggTarget.Position, pawn.Map, true, null, 0, 0) && IntVec3Utility.ManhattanDistanceFlat(pawn.PositionHeld, eggTarget.PositionHeld) < 30)
                        {
                            result = target;
                        }
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                Thing target = pawn.mindState.enemyTarget;
                Pawn pawnTarget = target as Pawn;
                Building_Turret turretTarget = target as Building_Turret;
                Egg eggTarget = target as Egg;
                Thing attackedTarget = null;
                if (pawnTarget != null && pawnTarget.mindState != null)
                {
                    attackedTarget = pawnTarget.mindState.lastAttackedTarget.Thing;
                }
                else if (turretTarget != null)
                {
                    attackedTarget = turretTarget.LastAttackedTarget.Thing;
                }
                else if (eggTarget != null && GenSight.LineOfSight(pawn.Position, eggTarget.Position, pawn.Map, true, null, 0, 0) && IntVec3Utility.ManhattanDistanceFlat(pawn.PositionHeld, eggTarget.PositionHeld) < 30)
                {
                    result = target;
                }
                if (attackedTarget != null)
                {
                    bool attackingHive = attackedTarget.Faction != null && attackedTarget.Faction == pawn.Faction;
                    result = GetAttacker(pawn, target, attackingHive);
                }
                if (result == null)
                {
                    pawn.mindState.enemyTarget = null;
                }
            }
            if (result != null)
            {
                if (result.def != HiveUtility.ThingDefOfEgg)
                {
                    pawn.mindState.enemyTarget = result;
                }
                List<Pawn> insectPawns = new List<Pawn>();
                foreach (Thing thing in allThings)
                {
                    Pawn insect = thing as Pawn;
                    if (!insect.DestroyedOrNull() && !insect.Downed && insect.Faction != null && insect.Faction == pawn.Faction && insect != pawn && insect.mindState.enemyTarget == null)
                    {
                        insectPawns.Add(insect);
                    }
                }
                foreach (Pawn insect in insectPawns)
                {
                    int dist = IntVec3Utility.ManhattanDistanceFlat(insect.PositionHeld, result.PositionHeld);
                    if (insect.CanReach(result, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors) && dist < maxAttackDistance)
                    {
                        if (result.def != HiveUtility.ThingDefOfEgg)
                        {
                            insect.mindState.enemyTarget = result;
                        }
                    }
                }
            }
            return result;
        }
        public static Thing GetAttacker(Pawn pawn, Thing target, bool attackingHive)
        {
            Pawn pawnTarget = target as Pawn;
            Insect enemyBug = target as Insect;
            Building_Turret turretTarget = target as Building_Turret;
            bool recentAttack = false;
            if (pawnTarget != null)
            {
                recentAttack = pawnTarget.mindState.lastAttackTargetTick <= Find.TickManager.TicksGame && pawnTarget.mindState.lastAttackTargetTick > Find.TickManager.TicksGame - ticksInAttackMode;
            }
            else if (turretTarget != null)
            {
                recentAttack = turretTarget.LastAttackTargetTick <= Find.TickManager.TicksGame && turretTarget.LastAttackTargetTick > Find.TickManager.TicksGame - ticksInAttackMode;
            }
            int dist = IntVec3Utility.ManhattanDistanceFlat(pawn.PositionHeld, target.PositionHeld);
            bool withinHiveAndInVisual = JobGiver_InsectGather.WithinHive(pawn, target, false) && GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map, true, null, 0, 0);
            bool enemyPawnThreat = (withinHiveAndInVisual || (attackingHive && recentAttack)) && dist < maxAttackDistance;
            bool enemyInsectThreat = enemyBug != null && enemyBug.Faction != pawn.Faction && GenSight.LineOfSight(pawn.Position, enemyBug.Position, pawn.Map, true, null, 0, 0) && dist < 30;
            if (enemyPawnThreat || enemyInsectThreat)
            {
                if (pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.PassDoors))
                {
                    return target;
                }
            }
            return null;
        }
    }
}
