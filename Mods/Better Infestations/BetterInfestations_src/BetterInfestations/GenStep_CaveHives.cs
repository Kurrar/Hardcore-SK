using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace BetterInfestations
{
	public class GenStep_CaveHives : GenStep
	{
		private List<IntVec3> rockCells = new List<IntVec3>();
		private List<IntVec3> possibleSpawnCells = new List<IntVec3>();
		private List<Queen> spawnedQueens = new List<Queen>();

		public override int SeedPart
		{
			get
			{
				return 349641510;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!Find.Storyteller.difficulty.allowCaveHives)
			{
				return;
			}
            if (BetterInfestationsMod.settings == null)
            {
                return;
            }
            if (!BetterInfestationsMod.settings.allowHiveMapGen)
            {
                return;
            }
            MapGenFloatGrid caves = MapGenerator.Caves;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			float num = 0.7f;
			int num2 = 0;
			rockCells.Clear();
			foreach (IntVec3 current in map.AllCells)
			{
				if (elevation[current] > num)
				{
					rockCells.Add(current);
				}
				if (caves[current] > 0f)
				{
					num2++;
				}
			}
			List<IntVec3> list = (from c in map.AllCells
			where map.thingGrid.ThingsAt(c).Any((Thing thing) => thing.Faction != null)
			select c).ToList();
			GenMorphology.Dilate(list, 50, map, null);
			HashSet<IntVec3> hashSet = new HashSet<IntVec3>(list);
			int num3 = GenMath.RoundRandom(num2 / 1000f);
            if (num3 > BetterInfestationsMod.settings.maxMapGenHives)
            {
                num3 = BetterInfestationsMod.settings.maxMapGenHives;
            }
			GenMorphology.Erode(rockCells, 10, map, null);
			possibleSpawnCells.Clear();
			for (int i = 0; i < rockCells.Count; i++)
			{
				if (caves[rockCells[i]] > 0f && !hashSet.Contains(rockCells[i]))
				{
					possibleSpawnCells.Add(rockCells[i]);
				}
			}
			spawnedQueens.Clear();
			for (int j = 0; j < num3; j++)
			{
				TrySpawnHive(map);
			}
			spawnedQueens.Clear();
		}

		private void TrySpawnHive(Map map)
		{
            IntVec3 intVec;
			if (!TryFindQueenSpawnCell(map, out intVec))
			{
				return;
			}
            possibleSpawnCells.Remove(intVec);

            Faction faction = HiveUtility.GetRandomInsectFaction();
            PawnKindDef kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Queen_Brown"), faction);
            Queen queen = PawnGenerator.GeneratePawn(kindDef, faction) as Queen;
            queen.hiveLocation = intVec;
            GenSpawn.Spawn(queen, CellFinder.RandomClosewalkCellNear(intVec, map, 4, null), map);
            spawnedQueens.Add(queen);
            Lord lord = queen.Lord;
            if (lord == null)
            {
                lord = queen.CreateNewLord();
            }
            int count = Rand.Range(4, 5);
            for (int i = 0; i < count; i++)
            {
                kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Megascarab_Brown"), faction);
                Pawn pawn = PawnGenerator.GeneratePawn(kindDef, faction);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(intVec, map, 4, null), map);
                queen.spawnedInsects.Add(pawn);
                lord.AddPawn(pawn);
            }
            IntVec3 c;
            int foodAmt = 0;
            for (int i = 0; i < Rand.Range(4, 7); i++)
            {
                if (CellFinder.TryFindRandomReachableCellNear(intVec, map, 8, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Walkable(map), (Region x) => true, out c, 999999))
                {
                    Egg egg = ThingMaker.MakeThing(HiveUtility.ThingDefOfEgg, null) as Egg;
                    egg.SetFaction(queen.Faction, null);
                    egg.eggLayer = queen;
                    GenPlace.TryPlaceThing(egg, c, map, ThingPlaceMode.Direct, out Thing t, null, null);
                    queen.spawnedEggs.Add(egg);
                    CompInsectSpawner comp = egg.TryGetComp<CompInsectSpawner>();
                    if (comp != null)
                    {
                        comp.jellyStores = Rand.Range(50, comp.jellyMax);
                    }
                    if (Rand.Range(1, 4) < 4)
                    {
                        CompSpawner comp2 = egg.TryGetComp<CompSpawner>();
                        if (comp2 != null)
                        {
                            comp2.TryDoSpawn();
                        }
                    }
                    if (foodAmt < (BetterInfestationsMod.settings.foodStorage + 100))
                    {
                        IntVec3 c2;
                        if (CellFinder.TryFindRandomReachableCellNear(intVec, map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Walkable(map), (Region x) => true, out c2, 999999))
                        {
                            Thing thing2 = ThingMaker.MakeThing(ThingDefOf.InsectJelly, null);
                            thing2.stackCount = Rand.Range(15,75);
                            GenPlace.TryPlaceThing(thing2, c2, map, ThingPlaceMode.Direct, out Thing food, null);
                            if (food != null)
                            {
                                food.SetForbidden(true);
                            }
                        }
                    }
                }
            }
		}

		private bool TryFindQueenSpawnCell(Map map, out IntVec3 spawnCell)
		{
			float num = -1f;
			IntVec3 intVec = IntVec3.Invalid;
			for (int i = 0; i < 3; i++)
			{
				IntVec3 intVec2;
				if (!(from x in possibleSpawnCells
				where x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null
				select x).TryRandomElement(out intVec2))
				{
					break;
				}
				float num2 = -1f;
				for (int j = 0; j < spawnedQueens.Count; j++)
				{
					float num3 = intVec2.DistanceToSquared(spawnedQueens[j].Position);
					if (num2 < 0f || num3 < num2)
					{
						num2 = num3;
					}
				}
				if (!intVec.IsValid || num2 > num)
				{
					intVec = intVec2;
					num = num2;
				}
			}
			spawnCell = intVec;
			return spawnCell.IsValid;
		}
	}
}
