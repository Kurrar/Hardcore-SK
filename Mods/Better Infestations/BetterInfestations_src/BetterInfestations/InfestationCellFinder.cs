using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BetterInfestations
{
	public static class InfestationCellFinder
	{
		private struct LocationCandidate
		{
			public IntVec3 cell;

			public float score;

			public LocationCandidate(IntVec3 cell, float score)
			{
				this.cell = cell;
				this.score = score;
			}
		}

        public static bool spawnInColony = false;
		private static List<LocationCandidate> locationCandidates = new List<LocationCandidate>();
		private static Dictionary<Region, float> regionsDistanceToUnroofed = new Dictionary<Region, float>();
		private static ByteGrid closedAreaSize;
		private static ByteGrid distToColonyBuilding;
        //private static List<Pair<IntVec3, float>> tmpCachedInfestationChanceCellColors;
		private static HashSet<Region> tempUnroofedRegions = new HashSet<Region>();
		private static List<IntVec3> tmpColonyBuildingsLocs = new List<IntVec3>();
        private static List<KeyValuePair<IntVec3, float>> tmpDistanceResult = new List<KeyValuePair<IntVec3, float>>();

		public static bool TryFindCell(out IntVec3 cell, Map map)
		{
            if (BetterInfestationsMod.settings == null)
            {
                cell = IntVec3.Invalid;
                return false;
            }
            for (int i = 0; i < 3; i++)
            {
                CalculateLocationCandidates(map);
                if (!locationCandidates.Any())
                {
                    if (BetterInfestationsMod.settings.allowHiveSpawnColony)
                    {
                        spawnInColony = true;
                    }
                }
                else
                {
                    break;
                }
            }
			LocationCandidate locationCandidate;
			if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out locationCandidate))
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = CellFinder.FindNoWipeSpawnLocNear(locationCandidate.cell, map, HiveUtility.ThingDefOfQueen, Rot4.North, 2, (IntVec3 x) => GetScoreAt(x, map) > 0f && x.GetFirstThing(map, HiveUtility.ThingDefOfQueen) == null && x.GetFirstThing(map, HiveUtility.ThingDefOfTunnelQueenSpawner) == null);
			return true;
		}

		private static float GetScoreAt(IntVec3 cell, Map map)
		{
            if (BetterInfestationsMod.settings == null)
            {
                return 0f;
            }
            if ((!spawnInColony && distToColonyBuilding[cell] <= BetterInfestationsMod.settings.hiveDistFromColony) || (spawnInColony && distToColonyBuilding[cell] > 30f))
			{
				return 0f;
			}
            if (CalculateDistanceToHives(cell, map) <= 50)
            {
                return 0f;
            }
            if (!cell.Walkable(map))
			{
				return 0f;
			}
			if (cell.Fogged(map))
			{
				return 0f;
			}
			if (CellHasBlockingThings(cell, map))
			{
				return 0f;
			}
			if (!cell.Roofed(map))
			{
				return 0f;
			}
			Region region = cell.GetRegion(map, RegionType.Set_Passable);
			if (region == null)
			{
				return 0f;
			}
			if (closedAreaSize[cell] < 2)
			{
				return 0f;
			}
			float temperature = cell.GetTemperature(map);
			if (temperature < -17f)
			{
				return 0f;
			}
			float mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
			if (mountainousnessScoreAt < 0.17f)
			{
				return 0f;
			}
            int num = StraightLineDistToUnroofed(cell, map);
			float num2;
			if (!regionsDistanceToUnroofed.TryGetValue(region, out num2))
			{
				num2 = num * 1.15f;
			}
			else
			{
				num2 = Mathf.Min(num2, num * 4f);
			}
			num2 = Mathf.Pow(num2, 1.55f);
			float num3 = Mathf.InverseLerp(0f, 12f, num);
			float num5 = 1f - Mathf.Clamp(DistToBlocker(cell, map) / 11f, 0f, 0.6f);
			float num6 = Mathf.InverseLerp(-17f, -7f, temperature);
			float num7 = num2 * num3 * num5 * mountainousnessScoreAt * num6;
			num7 = Mathf.Pow(num7, 1.2f);
			if (num7 < 7f)
			{
				return 0f;
			}
			return num7;
		}
        /*
		public static void DebugDraw()
		{
			if (DebugViewSettings.drawInfestationChance)
			{
				if (tmpCachedInfestationChanceCellColors == null)
				{
					tmpCachedInfestationChanceCellColors = new List<Pair<IntVec3, float>>();
				}
				if (Time.frameCount % 8 == 0)
				{
					tmpCachedInfestationChanceCellColors.Clear();
					Map currentMap = Find.CurrentMap;
					CellRect cellRect = Find.CameraDriver.CurrentViewRect;
					cellRect.ClipInsideMap(currentMap);
					cellRect = cellRect.ExpandedBy(1);
					CalculateTraversalDistancesToUnroofed(currentMap);
					CalculateClosedAreaSizeGrid(currentMap);
					CalculateDistanceToColonyBuildingGrid(currentMap);
                    float num = 0.001f;
					for (int i = 0; i < currentMap.Size.z; i++)
					{
						for (int j = 0; j < currentMap.Size.x; j++)
						{
							IntVec3 cell = new IntVec3(j, 0, i);
							float scoreAt = GetScoreAt(cell, currentMap);
							if (scoreAt > num)
							{
								num = scoreAt;
							}
						}
					}
					for (int k = 0; k < currentMap.Size.z; k++)
					{
						for (int l = 0; l < currentMap.Size.x; l++)
						{
							IntVec3 intVec = new IntVec3(l, 0, k);
							if (cellRect.Contains(intVec))
							{
								float scoreAt2 = GetScoreAt(intVec, currentMap);
								if (scoreAt2 > 7.5f)
								{
									float second = GenMath.LerpDouble(7.5f, num, 0f, 1f, scoreAt2);
									tmpCachedInfestationChanceCellColors.Add(new Pair<IntVec3, float>(intVec, second));
								}
							}
						}
					}
				}
				for (int m = 0; m < tmpCachedInfestationChanceCellColors.Count; m++)
				{
					IntVec3 first = tmpCachedInfestationChanceCellColors[m].First;
					float second2 = tmpCachedInfestationChanceCellColors[m].Second;
					CellRenderer.RenderCell(first, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, second2), false));
				}
			}
			else
			{
				tmpCachedInfestationChanceCellColors = null;
			}
		}
        */

		private static void CalculateLocationCandidates(Map map)
		{
			locationCandidates.Clear();
			CalculateTraversalDistancesToUnroofed(map);
			CalculateClosedAreaSizeGrid(map);
			CalculateDistanceToColonyBuildingGrid(map);
            for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 cell = new IntVec3(j, 0, i);
					float scoreAt = GetScoreAt(cell, map);
					if (scoreAt > 0f)
					{
						locationCandidates.Add(new LocationCandidate(cell, scoreAt));
					}
				}
			}
		}

		private static bool CellHasBlockingThings(IntVec3 cell, Map map)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Pawn || thingList[i] is Egg || thingList[i] is TunnelQueenSpawner)
				{
					return true;
				}
				bool flag = thingList[i].def.category == ThingCategory.Building && thingList[i].def.passability == Traversability.Impassable;
				if (flag && GenSpawn.SpawningWipes(HiveUtility.ThingDefOfEgg, thingList[i].def))
				{
					return true;
				}
			}
			return false;
		}

		private static int StraightLineDistToUnroofed(IntVec3 cell, Map map)
		{
			int num = 2147483647;
			int i = 0;
			while (i < 4)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 facingCell = rot.FacingCell;
				int num2 = 0;
				int num3;
				while (true)
				{
					IntVec3 intVec = cell + facingCell * num2;
					if (!intVec.InBounds(map))
					{
						goto Block_1;
					}
					num3 = num2;
					if (NoRoofAroundAndWalkable(intVec, map))
					{
						break;
					}
					num2++;
				}
				IL_6F:
				if (num3 < num)
				{
					num = num3;
				}
				i++;
				continue;
				Block_1:
				num3 = 2147483647;
				goto IL_6F;
			}
			if (num == 2147483647)
			{
				return map.Size.x;
			}
			return num;
		}

		private static float DistToBlocker(IntVec3 cell, Map map)
		{
			int num = -2147483648;
			int num2 = -2147483648;
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 facingCell = rot.FacingCell;
				int num3 = 0;
				int num4;
				while (true)
				{
					IntVec3 c = cell + facingCell * num3;
					num4 = num3;
					if (!c.InBounds(map) || !c.Walkable(map))
					{
						break;
					}
					num3++;
				}
				if (num4 > num)
				{
					num2 = num;
					num = num4;
				}
				else if (num4 > num2)
				{
					num2 = num4;
				}
			}
			return Mathf.Min(num, num2);
		}

		private static bool NoRoofAroundAndWalkable(IntVec3 cell, Map map)
		{
			if (!cell.Walkable(map))
			{
				return false;
			}
			if (cell.Roofed(map))
			{
				return false;
			}
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 c = rot.FacingCell + cell;
				if (c.InBounds(map) && c.Roofed(map))
				{
					return false;
				}
			}
			return true;
		}

		private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < 700; i += 10)
			{
				IntVec3 c = cell + GenRadial.RadialPattern[i];
				if (c.InBounds(map))
				{
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
					{
						num += 1f;
					}
					else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
					{
						num += 0.5f;
					}
					num2++;
				}
			}
			return num / num2;
		}

		private static void CalculateTraversalDistancesToUnroofed(Map map)
		{
			tempUnroofedRegions.Clear();
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					Region region = intVec.GetRegion(map, RegionType.Set_Passable);
					if (region != null && NoRoofAroundAndWalkable(intVec, map))
					{
						tempUnroofedRegions.Add(region);
					}
				}
			}
			Dijkstra<Region>.Run(tempUnroofedRegions, (Region x) => x.Neighbors, (Region a, Region b) => Mathf.Sqrt(a.extentsClose.CenterCell.DistanceToSquared(b.extentsClose.CenterCell)), regionsDistanceToUnroofed, null);
			tempUnroofedRegions.Clear();
		}

		private static void CalculateClosedAreaSizeGrid(Map map)
		{
			if (closedAreaSize == null)
			{
				closedAreaSize = new ByteGrid(map);
			}
			else
			{
				closedAreaSize.ClearAndResizeTo(map);
			}
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					if (closedAreaSize[j, i] == 0 && !intVec.Impassable(map))
					{
						int area = 0;
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							area++;
						}, 2147483647, false, null);
						area = Mathf.Min(area, 255);
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							closedAreaSize[c] = (byte)area;
						}, 2147483647, false, null);
					}
				}
			}
		}

		private static void CalculateDistanceToColonyBuildingGrid(Map map)
		{
			if (distToColonyBuilding == null)
			{
				distToColonyBuilding = new ByteGrid(map);
			}
			else if (!distToColonyBuilding.MapSizeMatches(map))
			{
				distToColonyBuilding.ClearAndResizeTo(map);
			}
			distToColonyBuilding.Clear(255);
			tmpColonyBuildingsLocs.Clear();
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				tmpColonyBuildingsLocs.Add(allBuildingsColonist[i].Position);
			}
			Dijkstra<IntVec3>.Run(tmpColonyBuildingsLocs, (IntVec3 x) => DijkstraUtility.AdjacentCellsNeighborsGetter(x, map), delegate(IntVec3 a, IntVec3 b)
			{
				if (a.x == b.x || a.z == b.z)
				{
					return 1f;
				}
				return 1.41421354f;
			}, tmpDistanceResult, null);
			for (int j = 0; j < tmpDistanceResult.Count; j++)
			{
				distToColonyBuilding[tmpDistanceResult[j].Key] = (byte)Mathf.Min(tmpDistanceResult[j].Value, 254.999f);
			}
		}

        private static int CalculateDistanceToHives(IntVec3 cell, Map map)
        {
            int best = int.MaxValue;
            List<Thing> things = HiveUtility.ListQueens(map);
            if (things != null && things.Any())
            {
                foreach (Thing thing in things)
                {
                    Queen queen = thing as Queen;
                    if (queen != null && queen.hiveLocation != null && queen.hiveLocation.IsValid)
                    {
                        int dist = IntVec3Utility.ManhattanDistanceFlat(cell, queen.hiveLocation);
                        if (dist < best)
                        {
                            best = dist;
                        }
                    }
                }
            }
            return best;
        }
    }
}
