using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
	public class IncidentWorker_DeepDrillInfestation : IncidentWorker
	{
        private static List<Thing> tmpDrills = new List<Thing>();

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
            if (BetterInfestationsMod.settings == null)
            {
                return false;
            }
            Map map = (Map)parms.target;
            if (HiveUtility.TotalSpawnedInsectCount(map) >= BetterInfestationsMod.settings.maxInsectsMap)
            {
                return false;
            }
            IntVec3 hiveLoc;
            if (!InfestationCellFinder.TryFindCell(out hiveLoc, map))
            {
                return false;
            }
            Faction faction = HiveUtility.GetRandomInsectFaction();
            if (faction == null)
            {
                return false;
            }
            tmpDrills.Clear();
            DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, tmpDrills);
            if (tmpDrills != null && tmpDrills.Any())
            {
                foreach (Thing t in tmpDrills)
                {
                    if (map.reachability.CanReach(t.Position, hiveLoc, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
                    {
                        return true;
                    }
                }
            }
            return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
            if (BetterInfestationsMod.settings == null)
            {
                return false;
            }
            Map map = (Map)parms.target;
            if (HiveUtility.TotalSpawnedInsectCount(map) >= BetterInfestationsMod.settings.maxInsectsMap)
            {
                return false;
            }
            IntVec3 hiveLoc;
            if (!InfestationCellFinder.TryFindCell(out hiveLoc, map))
            {
                return false;
            }
            Faction faction = HiveUtility.GetRandomInsectFaction();
            if (faction == null)
            {
                return false;
            }
            Thing deepDrill = null;
            List<Thing> drills = new List<Thing>();
            tmpDrills.Clear();
            DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, tmpDrills);
            if (tmpDrills != null && tmpDrills.Any())
            {
                foreach (Thing t in tmpDrills)
                {
                    if (map.reachability.CanReach(t.Position, hiveLoc, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
                    {
                        drills.Add(t);
                    }
                }
            }
            if (drills == null || !drills.Any())
            {
                return false;
            }
            deepDrill = drills.RandomElement();
            TunnelQueenSpawner tunnelQueenSpawner = (TunnelQueenSpawner)ThingMaker.MakeThing(HiveUtility.ThingDefOfTunnelQueenSpawner, null);
            tunnelQueenSpawner.insectsPoints = Mathf.Clamp(parms.points * Rand.Range(0.3f, 0.6f), 200f, BetterInfestationsMod.settings.maxParamPointsDeep);
            tunnelQueenSpawner.hiveLocation = hiveLoc;
            tunnelQueenSpawner.queenFaction = faction;
            GenSpawn.Spawn(tunnelQueenSpawner, deepDrill.Position, map, WipeMode.FullRefund);
            deepDrill.TryGetComp<CompCreatesInfestations>().Notify_CreatedInfestation();
            if (BetterInfestationsMod.settings.showNotifications)
            {
                Find.LetterStack.ReceiveLetter("letterlabeldeepdrillinfestation".Translate(), "lettertextdeepdrillinfestation".Translate(), LetterDefOf.ThreatBig, tunnelQueenSpawner);
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
			return true;
		}
	}
}
