using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace BetterInfestations
{
	public class CompInsectSpawner : ThingComp
	{
        public int jellyStores = 0;
        public int jellyMax = 75;
        public CompProperties_SpawnerHives Props
		{
			get
			{
				return (CompProperties_SpawnerHives)props;
			}
		}
        public override string CompInspectStringExtra()
        {
            string txt = null;
            Egg egg = parent as Egg;
            if (egg != null)
            {
                if (jellyStores < jellyMax)
                {
                    txt = "egg_foodneeded_desc".Translate() + jellyStores + "/" + jellyMax;
                }
                else
                {
                    float curTick = Find.TickManager.TicksGame - egg.eggSpawnTick;
                    float endTick = egg.insectSpawnTick - egg.eggSpawnTick;
                    int perc = (int)(curTick / endTick * 100f);
                    txt = "egg_foodneeded_desc".Translate() + jellyStores + "/" + jellyMax + "\r\n" + "egg_spawning_desc".Translate() + perc + "%";
                }
            }
            return txt;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "egg_debug_desc".Translate(),
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate
                    {
                        Egg egg = parent as Egg;
                        if (egg != null)
                        {
                            SpawnBug(egg.Faction);
                        }
                    }
                };
            }
        }

        public void SpawnBug(Faction faction)
        {
            Egg egg = parent as Egg;
            Queen queen = egg.eggLayer;
            PawnKindDef kindDef = null;
            int random = Rand.Range(1, 50);
            int random2 = Rand.Range(1, 3);
            switch (random)
            {
                case int n when n <= 26:
                    kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Megascarab_Brown"), faction);
                    break;
                case int n when n >= 27 && n <= 40:
                    kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Spelopede_Brown"), faction);
                    break;
                case int n when n >= 41 && n <= 48:
                    kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Megaspider_Brown"), faction);
                    break;
                case int n when n >= 49 && n <= 50:
                    kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Queen_Brown"), faction);
                    break;
            }
            if (kindDef != null)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, -1, true, true, false, false, false, false, 1f, false, false, true, false, false, false, false, null, null, null, null, null, null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (kindDef.defName.Contains("BI_Queen"))
                {
                    IntVec3 loc;
                    if (InfestationCellFinder.TryFindCell(out loc, egg.Map))
                    {
                        Queen queen2 = pawn as Queen;
                        if (queen2 != null)
                        {
                            queen2.hiveLocation = loc;
                        }
                    }
                }
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(egg.Position, egg.Map, 2, null), egg.Map);
                Lord lord = null;
                if (queen != null && !queen.Dead)
                {
                    lord = queen.Lord;
                    queen.spawnedEggs.Remove(egg);
                    if (!kindDef.defName.Contains("BI_Queen"))
                    {
                        queen.spawnedInsects.Add(pawn);
                    }
                }
                if (queen == null || queen.Dead || lord == null)
                {
                    lord = CreateNewLord(faction);
                }
                if (!kindDef.defName.Contains("BI_Queen"))
                {
                    lord.AddPawn(pawn);
                }
                FilthMaker.MakeFilth(egg.Position, egg.Map, ThingDefOf.Filth_Slime, 12);
                SoundDefOf.Hive_Spawn.PlayOneShot(egg);
                egg.Destroy(DestroyMode.Vanish);
            }
        }
        public Lord CreateNewLord(Faction faction)
        {
            return LordMaker.MakeNewLord(faction, new LordJob_QueenKilled(), parent.Map, null);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref jellyStores, "jellyStores", 0, false);
        }
    }
}
