using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using UnityEngine;

namespace BetterInfestations
{
	public class Egg : ThingWithComps
	{
        public Queen eggLayer = null;
        public int insectSpawnTick = 0;
        public int eggSpawnTick = 0;

        public Graphic cachedGraphic;
        public GraphicData brownHiveGraphic = new GraphicData { texPath = "Things/Building/Natural/Hive", graphicClass = typeof(Graphic_Single), drawSize = new Vector2(1.0f, 1.0f) };
        public GraphicData redHiveGraphic = new GraphicData { texPath = "BI/Hive_Red", graphicClass = typeof(Graphic_Single), drawSize = new Vector2(1.0f, 1.0f) };
        public GraphicData blackHiveGraphic = new GraphicData { texPath = "BI/Hive_Black", graphicClass = typeof(Graphic_Single), drawSize = new Vector2(1.0f, 1.0f) };
        public GraphicData greenHiveGraphic = new GraphicData { texPath = "BI/Hive_Green", graphicClass = typeof(Graphic_Single), drawSize = new Vector2(1.0f, 1.0f) };

        public override Graphic Graphic
        {
            get
            {
                if (Faction != null)
                {
                    if (Faction.def.defName == "BI_InsectBrownFaction")
                    {
                        cachedGraphic = brownHiveGraphic.GraphicColoredFor(this);
                    }
                    else if (Faction.def.defName == "BI_InsectRedFaction")
                    {
                        cachedGraphic = redHiveGraphic.GraphicColoredFor(this);
                    }
                    else if (Faction.def.defName == "BI_InsectBlackFaction")
                    {
                        cachedGraphic = blackHiveGraphic.GraphicColoredFor(this);
                    }
                    else if (Faction.def.defName == "BI_InsectGreenFaction")
                    {
                        cachedGraphic = greenHiveGraphic.GraphicColoredFor(this);
                    }
                }
                return cachedGraphic;
            }
        }

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
            if (Faction == null)
			{
				SetFaction(HiveUtility.GetRandomInsectFaction(), null);
			}
		}

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Queen queen = eggLayer;
            if (queen != null)
            {
                if (queen.spawnedEggs.Any())
                {
                    queen.spawnedEggs.Remove(this);
                }
            }
            base.DeSpawn(mode);
        }

		public override void Tick()
		{
			base.Tick();
            if (Spawned)
            {
                CompInsectSpawner comp = this.TryGetComp<CompInsectSpawner>();
                if (comp != null)
                {
                    if (Find.TickManager.TicksGame >= insectSpawnTick && insectSpawnTick != 0)
                    {
                        comp.SpawnBug(Faction);
                    }
                    if (comp.jellyStores >= comp.jellyMax && insectSpawnTick == 0)
                    {
                        eggSpawnTick = Find.TickManager.TicksGame;
                        CalculateInsectSpawnTick();
                    }
                    else if (comp.jellyStores < comp.jellyMax)
                    {
                        if (Map.thingGrid != null)
                        {
                            Thing thing = Map.thingGrid.ThingAt(Position, ThingDefOf.InsectJelly);
                            if (thing != null)
                            {
                                int jellyNeeded = comp.jellyMax - comp.jellyStores;
                                int stackCount = thing.stackCount;
                                if (jellyNeeded >= stackCount)
                                {
                                    comp.jellyStores = comp.jellyStores + stackCount;
                                    thing.Destroy(DestroyMode.Vanish);
                                }
                                else if (jellyNeeded < stackCount)
                                {
                                    int stackAdj = stackCount - jellyNeeded;
                                    thing.stackCount = stackAdj;
                                    comp.jellyStores = comp.jellyMax;
                                }
                            }
                        }
                    }
                }
            }
		}

        public void CalculateInsectSpawnTick()
        {
            if (BetterInfestationsMod.settings == null)
            {
                return;
            }
            float interval = Find.TickManager.TicksGame + (BetterInfestationsMod.settings.eggHatchingRate * 60000f);
            insectSpawnTick = (int)interval;
        }

        public override void ExposeData()
		{
			base.ExposeData();
            Scribe_References.Look(ref eggLayer, "eggLayer", false);
            Scribe_Values.Look(ref insectSpawnTick, "insectSpawnTick", 0, false);
            Scribe_Values.Look(ref eggSpawnTick, "eggSpawnTick", 0, false);
        }
	}
}
