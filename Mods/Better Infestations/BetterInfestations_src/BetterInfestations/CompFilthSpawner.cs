using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;

namespace BetterInfestations
{
    public class CompFilthSpawner : ThingComp
    {
        private CompProperties_FilthSpawner Props
        {
            get
            {
                return (CompProperties_FilthSpawner)props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                for (int i = 0; i < Props.spawnCountOnSpawn; i++)
                {
                    TrySpawnFilth();
                }
            }
        }

        public void TrySpawnFilth()
        {
            if (parent.Map == null)
            {
                return;
            }
            IntVec3 c;
            if (!CellFinder.TryFindRandomReachableCellNear(parent.Position, parent.Map, Props.spawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Walkable(parent.Map), (Region x) => true, out c, 999999))
            {
                return;
            }
            TerrainDef terrainDef = parent.Map.terrainGrid.TerrainAt(c);
            if (terrainDef != null)
            {
                terrainDef.acceptFilth = true;
            }
            FilthMaker.MakeFilth(c, parent.Map, Props.filthDef, 1);
        }
    }
}
