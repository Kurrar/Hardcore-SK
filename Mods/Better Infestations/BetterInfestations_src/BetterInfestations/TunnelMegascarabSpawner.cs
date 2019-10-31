using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace BetterInfestations
{
	[StaticConstructorOnStartup]
	public class TunnelMegascarabSpawner : ThingWithComps
	{
		public int secondarySpawnTick;
        public Sustainer sustainer;
        public Faction faction = null;
        public Queen queen = null;
        public static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
        public readonly FloatRange ResultSpawnDelay = new FloatRange(12f, 16f);

		[TweakValue("Gameplay", 0f, 1f)]
        public static float DustMoteSpawnMTB = 0.2f;

		[TweakValue("Gameplay", 0f, 1f)]
        public static float FilthSpawnMTB = 0.3f;

		[TweakValue("Gameplay", 0f, 10f)]
        public static float FilthSpawnRadius = 3f;

        public static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);
        public static List<ThingDef> filthTypes = new List<ThingDef> { ThingDefOf.Filth_Dirt, ThingDefOf.Filth_Dirt, ThingDefOf.Filth_Dirt, ThingDefOf.Filth_RubbleRock };

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref faction, "faction", null, false);
            Scribe_Values.Look(ref queen, "queen", null, false);
        }

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				secondarySpawnTick = Find.TickManager.TicksGame + ResultSpawnDelay.RandomInRange.SecondsToTicks();
			}
			CreateSustainer();
		}

		public override void Tick()
		{
			if (Spawned)
			{
				sustainer.Maintain();
				Vector3 vector = Position.ToVector3Shifted();
				IntVec3 c;
				if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(Position, Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c, 999999))
				{
					FilthMaker.MakeFilth(c, Map, filthTypes.RandomElement(), 1);
				}
				if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
				{
					MoteMaker.ThrowDustPuffThick(new Vector3(vector.x, 0f, vector.z)
					{
						y = AltitudeLayer.MoteOverhead.AltitudeFor()
					}, Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
				}
				if (secondarySpawnTick <= Find.TickManager.TicksGame)
				{
					sustainer.End();
					Map map = Map;
                    IntVec3 position = Position;
					Destroy(DestroyMode.Vanish);

                    if (faction == null)
                    {
                        faction = HiveUtility.GetRandomInsectFaction();
                    }
                    PawnKindDef kindDef = HiveUtility.GetFactionKindDef(PawnKindDef.Named("BI_Megascarab_Brown"), faction);
                    Insect insect = PawnGenerator.GeneratePawn(kindDef, faction) as Insect;
                    insect.stealFood = true;
                    GenSpawn.Spawn(insect, CellFinder.RandomClosewalkCellNear(position, map, 4, null), map);
                    if (queen != null)
                    {
                        queen.spawnedInsects.Add(insect);
                        Lord lord = queen.Lord;
                        if (lord == null)
                        {
                            lord = queen.CreateNewLord();
                        }
                        lord.AddPawn(insect);
                    }
                }
			}
		}

		public override void Draw()
		{
			Rand.PushState();
			Rand.Seed = thingIDNumber;
			for (int i = 0; i < 6; i++)
			{
				DrawDustPart(Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * Rand.Sign * 4f, Rand.Range(1f, 1.5f));
			}
			Rand.PopState();
		}

        public void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
		{
			float num = (Find.TickManager.TicksGame - secondarySpawnTick).TicksToSeconds();
			Vector3 pos = Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
			pos.y += 0.046875f * Rand.Range(0f, 1f);
			Color value = new Color(0.470588237f, 0.384313732f, 0.3254902f, 0.7f);
            matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
			Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelMaterial, 0, null, 0, matPropertyBlock);
		}

        public void CreateSustainer()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				SoundDef tunnel = SoundDefOf.Tunnel;
				sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}
	}
}
