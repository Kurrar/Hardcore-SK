using RimWorld;
using UnityEngine;
using Verse;

namespace BetterInfestations
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
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
            IntVec3 loc;
            if (!InfestationCellFinder.TryFindCell(out loc, map))
            {
                return false;
            }
            Faction faction = HiveUtility.GetRandomInsectFaction();
            if (faction == null)
            {
                return false;
            }
            return true;
        }

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
            if (BetterInfestationsMod.settings != null)
            {
                Map map = (Map)parms.target;
                int hiveCount = Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1);
                if (hiveCount > BetterInfestationsMod.settings.maxQueens)
                {
                    hiveCount = BetterInfestationsMod.settings.maxQueens;
                }
                Thing t = SpawnTunnels(hiveCount, map, parms);
                if (t != null)
                {
                    if (BetterInfestationsMod.settings.showNotifications)
                    {
                        Find.LetterStack.ReceiveLetter("letterlabelinfestation".Translate(), "lettertextinfestation".Translate(), LetterDefOf.ThreatBig, t);
                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                    }
                    return true;
                }
            }
			return false;
        }

		private Thing SpawnTunnels(int hiveCount, Map map, IncidentParms parms)
		{
            if (BetterInfestationsMod.settings == null)
            {
                return null;
            }
            if (HiveUtility.TotalSpawnedInsectCount(map) >= BetterInfestationsMod.settings.maxInsectsMap)
            {
                return null;
            }
            IntVec3 loc;
			if (!InfestationCellFinder.TryFindCell(out loc, map))
			{
				return null;
			}
            Faction faction = HiveUtility.GetRandomInsectFaction();
            if (faction == null)
            {
                return null;
            }
            TunnelQueenSpawner tunnelQueenSpawner = ThingMaker.MakeThing(HiveUtility.ThingDefOfTunnelQueenSpawner, null) as TunnelQueenSpawner;
            tunnelQueenSpawner.queenFaction = faction;
            if (BetterInfestationsMod.settings.spawnAdditionalInsects)
            {
                tunnelQueenSpawner.insectsPoints = Mathf.Clamp(parms.points * Rand.Range(0.3f, 0.6f), 200f, BetterInfestationsMod.settings.maxParamPointsInf);
            }
            GenSpawn.Spawn(tunnelQueenSpawner, loc, map, WipeMode.FullRefund);
			for (int i = 0; i < hiveCount - 1; i++)
			{
                IntVec3 c;
                if (CellFinder.TryFindRandomReachableCellNear(loc, map, 12, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Walkable(map), (Region x) => true, out c, 999999))
                {
                    if (c.IsValid)
                    {
                        TunnelQueenSpawner tunnelQueenSpawner2 = ThingMaker.MakeThing(HiveUtility.ThingDefOfTunnelQueenSpawner, null) as TunnelQueenSpawner;
                        tunnelQueenSpawner2.queenFaction = faction;
                        if (BetterInfestationsMod.settings.spawnAdditionalInsects)
                        {
                            tunnelQueenSpawner2.insectsPoints = Mathf.Clamp(parms.points * Rand.Range(0.3f, 0.6f), 200f, BetterInfestationsMod.settings.maxParamPointsInf);
                        }
                        GenSpawn.Spawn(tunnelQueenSpawner2, c, map, WipeMode.FullRefund);
                    }
                }
			}
			return tunnelQueenSpawner;
        }
	}
}
