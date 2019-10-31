using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;

namespace BetterInfestations
{
	public class IncidentWorker_FoodInfestation : IncidentWorker
	{
        Zone targetZone = null;
        Queen queenMother = null;
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
            if (!GetTargetZone(map))
            {
                return false;
            }
            if (!GetQueen(map))
            {
                return false;
            }
            return true;
		}

        public bool GetQueen(Map map)
        {
            if (BetterInfestationsMod.settings == null)
            {
                return false;
            }
            if (targetZone == null)
            {
                return false;
            }
            if (!targetZone.cells.Any())
            {
                return false;
            }
            List<Thing> things = HiveUtility.ListQueens(map);
            if (things != null && things.Any())
            {
                foreach (Thing thing in things)
                {
                    Queen queen = thing as Queen;
                    if (queen != null)
                    {
                        if (map.reachability.CanReach(targetZone.cells.RandomElement(), queen.hiveLocation, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
                        {
                            float insectCount = HiveUtility.TotalHiveInsectCount(queen);
                            float maxInsectsHive = BetterInfestationsMod.settings.maxInsectsHive;
                            if (insectCount >= maxInsectsHive)
                            {
                                queenMother = queen;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool GetTargetZone(Map map)
        {
            List<Zone> zones = map.zoneManager.AllZones;
            if (zones != null && zones.Any())
            {
                foreach (Zone zone in zones)
                {
                    Zone_Stockpile stockpile = zone as Zone_Stockpile;
                    if (stockpile != null)
                    {
                        List<IntVec3> cells = zone.cells as List<IntVec3>;
                        if (cells == null || !cells.Any())
                        {
                            continue;
                        }
                        int foodAmount = 0;
                        foreach (IntVec3 cell in cells)
                        {
                            Thing thing = map.thingGrid.ThingAt(cell, ThingCategory.Item);
                            if (thing != null && !thing.def.IsCorpse && thing.IngestibleNow)
                            {
                                foodAmount += thing.stackCount;
                                if (foodAmount >= 300)
                                {
                                    targetZone = zone as Zone;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
            Map map = (Map)parms.target;
            if (!GetTargetZone(map))
            {
                return false;
            }
            if (!GetQueen(map))
            {
                return false;
            }
            if (BetterInfestationsMod.settings != null)
            {
                Thing t = SpawnTunnels(map);
                if (t != null)
                {
                    if (BetterInfestationsMod.settings.showNotifications && !queenMother.colonyFoodFound)
                    {
                        Find.LetterStack.ReceiveLetter("letterlabelfoodinfestation".Translate(), "lettertextfoodinfestation".Translate(), LetterDefOf.ThreatBig, t);
                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                    }
                    return true;
                }
            }
			return false;
        }

		private Thing SpawnTunnels(Map map)
		{
            TunnelMegascarabSpawner tunnelMegascarabSpawner = ThingMaker.MakeThing(HiveUtility.ThingDefOfTunnelMegascarabSpawner, null) as TunnelMegascarabSpawner;
            tunnelMegascarabSpawner.queen = queenMother;
            tunnelMegascarabSpawner.faction = queenMother.Faction;
            IntVec3 loc = targetZone.cells.RandomElement();
            queenMother.colonyFoodLoc = loc;
            GenSpawn.Spawn(tunnelMegascarabSpawner, loc, map, WipeMode.FullRefund);
            for (int i = 0; i < Rand.Range(3, 5) - 1; i++)
            {
                IntVec3 c;
                if (CellFinder.TryFindRandomReachableCellNear(loc, map, 5, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Walkable(map), (Region x) => true, out c, 999999))
                {
                    if (c.IsValid)
                    {
                        TunnelMegascarabSpawner tunnelMegascarabSpawner2 = ThingMaker.MakeThing(HiveUtility.ThingDefOfTunnelMegascarabSpawner, null) as TunnelMegascarabSpawner;
                        tunnelMegascarabSpawner2.faction = queenMother.Faction;
                        tunnelMegascarabSpawner2.queen = queenMother;
                        GenSpawn.Spawn(tunnelMegascarabSpawner2, c, map, WipeMode.FullRefund);
                    }
                }
            }
			return tunnelMegascarabSpawner;
        }
	}
}
