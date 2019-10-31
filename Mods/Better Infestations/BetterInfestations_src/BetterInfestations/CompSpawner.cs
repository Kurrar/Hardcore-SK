using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterInfestations
{
	public class CompSpawner : ThingComp
	{
		private int ticksUntilSpawn;

		public CompProperties_Spawner PropsSpawner
		{
			get
			{
				return (CompProperties_Spawner)props;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				ResetCountdown();
			}
		}

		public override void CompTick()
		{
			TickInterval(1);
		}

		public override void CompTickRare()
		{
			TickInterval(250);
		}

		private void TickInterval(int interval)
		{
			if (!parent.Spawned)
			{
				return;
			}
			if (parent.Position.Fogged(parent.Map))
			{
				return;
			}
			ticksUntilSpawn -= interval;
			CheckShouldSpawn();
		}

		private void CheckShouldSpawn()
		{
			if (ticksUntilSpawn <= 0)
			{
				TryDoSpawn();
				ResetCountdown();
			}
		}

		public bool TryDoSpawn()
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (PropsSpawner.spawnMaxAdjacent >= 0)
			{
				int num = 0;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
					if (c.InBounds(parent.Map))
					{
						List<Thing> thingList = c.GetThingList(parent.Map);
						for (int j = 0; j < thingList.Count; j++)
						{
							if (thingList[j].def == PropsSpawner.thingToSpawn)
							{
								num += thingList[j].stackCount;
								if (num >= PropsSpawner.spawnMaxAdjacent)
								{
									return false;
								}
							}
						}
					}
				}
			}
			IntVec3 center;
			if (TryFindSpawnCell(out center))
			{
				Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn, null);
				thing.stackCount = PropsSpawner.spawnCount;
				if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
				{
					thing.SetFaction(parent.Faction, null);
				}
				Thing t;
                GenPlace.TryPlaceThing(thing, center, parent.Map, ThingPlaceMode.Direct, out t, null, null);
				if (PropsSpawner.spawnForbidden)
				{
					t.SetForbidden(true, true);
				}
                if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
				{
					Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent, true);
				}
				return true;
			}
			return false;
		}

		private bool TryFindSpawnCell(out IntVec3 result)
		{
			foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(parent).InRandomOrder(null))
			{
				if (current.Walkable(parent.Map))
				{
					Building edifice = current.GetEdifice(parent.Map);
					if (edifice == null || !PropsSpawner.thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if (building_Door == null || building_Door.FreePassage)
						{
							if (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, current, parent.Map, false, null, 0, 0))
							{
								bool flag = false;
								List<Thing> thingList = current.GetThingList(parent.Map);
								for (int i = 0; i < thingList.Count; i++)
								{
									Thing thing = thingList[i];
									if (thing.def.category == ThingCategory.Item && (thing.def != PropsSpawner.thingToSpawn || thing.stackCount > PropsSpawner.thingToSpawn.stackLimit - PropsSpawner.spawnCount))
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									result = current;
									return true;
								}
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		private void ResetCountdown()
		{
			ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		public override void PostExposeData()
		{
			string str = (!PropsSpawner.saveKeysPrefix.NullOrEmpty()) ? (PropsSpawner.saveKeysPrefix + "_") : null;
			Scribe_Values.Look(ref ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
		}

		public override string CompInspectStringExtra()
		{
			if (PropsSpawner.writeTimeLeftToSpawn)
			{
				return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)) + ": " + ticksUntilSpawn.ToStringTicksToPeriod();
			}
			return null;
		}
	}
}
