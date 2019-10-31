using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace BetterInfestations
{
    public class Queen : Pawn
	{
        public int manageTick = 0;
        public int nextEggSpawnTick = 0;
        public List<Pawn> spawnedInsects = new List<Pawn>();
        public List<Egg> spawnedEggs = new List<Egg>();
        public bool eggReady = false;
        public bool colonyFoodFound = false;
        public IntVec3 colonyFoodLoc = IntVec3.Invalid;
        public IntVec3 hiveLocation = IntVec3.Invalid;

        public static readonly string MemoAttackedByEnemy = "QueenAttacked";
        public static readonly string MemoDeSpawned = "QueenDeSpawned";
        public static readonly string MemoDestroyedNonRoofCollapse = "QueenDestroyedNonRoofCollapse";

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                CalculateNextEggSpawnTick();
                gender = Gender.Female;
                if (Faction == null)
                {
                    SetFaction(HiveUtility.GetInsectFaction(this));
                }
                if (!hiveLocation.IsValid)
                {
                    hiveLocation = Position;
                    IntVec3 loc;
                    if (InfestationCellFinder.TryFindCell(out loc, map))
                    {
                        hiveLocation = loc;
                    }
                }
                spawnedInsects.Add(this);
                Lord lord = Lord;
                if (lord == null)
                {
                    lord = CreateNewLord();
                }
                lord.AddPawn(this);
            }
        }
        public Lord Lord
        {
            get
            {
                Predicate<Pawn> hasQueenLord = delegate (Pawn x)
                {
                    Lord lord = x.GetLord();
                    return lord != null;
                };
                Pawn foundPawn = spawnedInsects.Find(hasQueenLord);
                if (Spawned)
                {
                    if (foundPawn != null)
                    {
                        return foundPawn.GetLord();
                    }
                }
                return null;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Spawned)
            {
                FilterOutUnspawned();
                if (Find.TickManager.TicksGame >= nextEggSpawnTick && !eggReady)
                {
                    eggReady = true;
                    CalculateNextEggSpawnTick();
                }
                if (Find.TickManager.TicksGame >= manageTick)
                {
                    ManageHive();
                    manageTick = Find.TickManager.TicksGame + 180;
                }
            }
        }

        public void CalculateNextEggSpawnTick()
        {
            if (BetterInfestationsMod.settings == null)
            {
                return;
            }
            float interval = Find.TickManager.TicksGame + (BetterInfestationsMod.settings.eggLayingRate * 60000f);
            nextEggSpawnTick = (int)interval;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Lord lord = Lord;
            if (lord != null)
            {
                lord.ReceiveMemo(MemoDeSpawned);
            }
            base.DeSpawn(mode);
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            Lord lord = Lord;
            if (lord != null)
            {
                if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null && dinfo.Instigator.Faction == Faction.OfPlayer)
                {
                    lord.ReceiveMemo(MemoAttackedByEnemy);
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {
            Lord lord = Lord;
            if (lord != null)
            {
                if (Spawned && (!dinfo.HasValue || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
                {
                    lord.ReceiveMemo(MemoDestroyedNonRoofCollapse);
                }
            }
            base.Kill(dinfo, exactCulprit);
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "queen_debug_desc".Translate(),
                    icon = TexCommand.GatherSpotActive,
                    action = delegate
                    {
                        if (HiveUtility.TotalHiveInsectCount(this) >= BetterInfestationsMod.settings.maxInsectsHive)
                        {
                            Messages.Message("queen_debug_msg1_desc".Translate() + BetterInfestationsMod.settings.maxInsectsHive, MessageTypeDefOf.RejectInput, false);
                        }
                        if (HiveUtility.TotalSpawnedInsectCount(Map) >= BetterInfestationsMod.settings.maxInsectsMap)
                        {
                            Messages.Message("queen_debug_msg2_desc".Translate() + BetterInfestationsMod.settings.maxInsectsMap, MessageTypeDefOf.RejectInput, false);
                        }
                        eggReady = true;
                        CalculateNextEggSpawnTick();
                    }
                };
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(MainDesc(true));
            if (carryTracker != null && carryTracker.CarriedThing != null)
            {
                stringBuilder.Append("Carrying".Translate() + ": ");
                stringBuilder.AppendLine(carryTracker.CarriedThing.LabelCap);
            }
            string text = null;
            Lord lord = this.GetLord();
            if (lord != null && lord.LordJob != null)
            {
                text = lord.LordJob.GetReport();
            }
            if (jobs.curJob != null)
            {
                try
                {
                    string text2 = jobs.curDriver.GetReport().CapitalizeFirst();
                    if (!text.NullOrEmpty())
                    {
                        text = text + ": " + text2;
                    }
                    else
                    {
                        text = text2;
                    }
                }
                catch (Exception arg)
                {
                    Log.Error("JobDriver.GetReport() exception: " + arg, false);
                }
            }
            if (!text.NullOrEmpty())
            {
                stringBuilder.AppendLine(text);
            }
            stringBuilder.Append(InspectStringPartsFromComps());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref manageTick, "manageTick", 0, false);
            Scribe_Values.Look(ref nextEggSpawnTick, "nextEggSpawnTick", 0, false);
            Scribe_Collections.Look(ref spawnedInsects, "spawnedInsects", LookMode.Reference, new object[0]);
            Scribe_Collections.Look(ref spawnedEggs, "spawnedEggs", LookMode.Reference, new object[0]);
            Scribe_Values.Look(ref eggReady, "eggReady", false, false);
            Scribe_Values.Look(ref colonyFoodFound, "colonyFoodFound", false, false);
            Scribe_Values.Look(ref colonyFoodLoc, "colonyFoodLoc", IntVec3.Invalid, false);
            Scribe_Values.Look(ref hiveLocation, "hiveLocation", IntVec3.Invalid, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                spawnedInsects.RemoveAll((Pawn x) => x == null);
                spawnedEggs.RemoveAll((Egg x) => x == null);
            }
        }
        public void FilterOutUnspawned()
        {
            for (int i = spawnedInsects.Count - 1; i >= 0; i--)
            {
                if (!spawnedInsects[i].Spawned)
                {
                    spawnedInsects.RemoveAt(i);
                }
                else if (spawnedInsects[i].GetLord() == null && !spawnedInsects[i].Downed && Lord != null && Lord.ownedPawns != null && !Lord.ownedPawns.Contains(spawnedInsects[i]))
                {
                    Lord.AddPawn(spawnedInsects[i]);
                }
            }
            for (int i = spawnedEggs.Count - 1; i >= 0; i--)
            {
                if (!spawnedEggs[i].Spawned)
                {
                    spawnedEggs.RemoveAt(i);
                }
            }
        }
        public override bool PreventPlayerSellingThingsNearby(out string reason)
        {
            if (spawnedInsects.Count > 0)
            {
                if (spawnedInsects.Any((Pawn p) => !p.Downed))
                {
                    reason = def.label;
                    return true;
                }
            }
            reason = null;
            return false;
        }
        public Lord CreateNewLord()
        {
            return LordMaker.MakeNewLord(Faction, new LordJob_DefendAndExpandHive(), Map, null);
        }
        public void ManageHive()
        {
            if (BetterInfestationsMod.settings == null)
            {
                return;
            }
            if (!spawnedInsects.Any())
            {
                return;
            }
            int[] workerCount = new int[3];
            List<List<Pawn>> insectList = new List<List<Pawn>>();
            for (int i = 0; i < 3; i++)
            {
                insectList.Add(new List<Pawn>());
            }
            for (int i = 0; i < spawnedInsects.Count; i++)
            {
                if (HiveUtility.megascarabKindDef.Contains(spawnedInsects[i].kindDef))
                {
                    insectList[0].Add(spawnedInsects[i]);
                }
                else if (HiveUtility.spelopedeKindDef.Contains(spawnedInsects[i].kindDef))
                {
                    insectList[1].Add(spawnedInsects[i]);
                }
                else if (HiveUtility.megaspiderKindDef.Contains(spawnedInsects[i].kindDef))
                {
                    insectList[2].Add(spawnedInsects[i]);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                if (!insectList[i].Any())
                {
                    continue;
                }
                workerCount[i] = (int)Math.Round(insectList[i].Count * BetterInfestationsMod.settings.hiveAggression, MidpointRounding.AwayFromZero);
                for (int j = 0; j < insectList[i].Count; j++)
                {
                    Insect insect = insectList[i][j] as Insect;
                    if (insect == null)
                    {
                        continue;
                    }
                    if (insect.worker)
                    {
                        if (workerCount[i] > 0)
                        {
                            if (colonyFoodFound && HiveUtility.TotalHiveInsectCount(this) >= BetterInfestationsMod.settings.maxInsectsHive)
                            {
                                insect.targetColonyFood = true;
                                insect.stealFood = true;
                            }
                            workerCount[i]--;
                        }
                        else
                        {
                            insect.worker = false;
                            insect.targetColonyFood = false;
                            insect.stealFood = false;
                        }
                    }
                    else
                    {
                        if (workerCount[i] > 0)
                        {
                            insect.worker = true;
                            workerCount[i]--;
                        }
                    }
                }
            }
        }
    }
}
