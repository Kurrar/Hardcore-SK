using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterInfestations
{
    public static class HiveUtility
    {
        public static PawnKindDef[] insect_BrownKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Megaspider_Brown"), PawnKindDef.Named("BI_Spelopede_Brown"), PawnKindDef.Named("BI_Megascarab_Brown"), PawnKindDef.Named("BI_Queen_Brown") };
        public static PawnKindDef[] insect_RedKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Megaspider_Red"), PawnKindDef.Named("BI_Spelopede_Red"), PawnKindDef.Named("BI_Megascarab_Red"), PawnKindDef.Named("BI_Queen_Red") };
        public static PawnKindDef[] insect_BlackKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Megaspider_Black"), PawnKindDef.Named("BI_Spelopede_Black"), PawnKindDef.Named("BI_Megascarab_Black"), PawnKindDef.Named("BI_Queen_Black") };
        public static PawnKindDef[] insect_GreenKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Megaspider_Green"), PawnKindDef.Named("BI_Spelopede_Green"), PawnKindDef.Named("BI_Megascarab_Green"), PawnKindDef.Named("BI_Queen_Green") };

        public static PawnKindDef[] queenKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Queen_Brown"), PawnKindDef.Named("BI_Queen_Red"), PawnKindDef.Named("BI_Queen_Black"), PawnKindDef.Named("BI_Queen_Green") };
        public static PawnKindDef[] megaspiderKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Megaspider_Brown"), PawnKindDef.Named("BI_Megaspider_Red"), PawnKindDef.Named("BI_Megaspider_Black"), PawnKindDef.Named("BI_Megaspider_Green") };
        public static PawnKindDef[] spelopedeKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Spelopede_Brown"), PawnKindDef.Named("BI_Spelopede_Red"), PawnKindDef.Named("BI_Spelopede_Black"), PawnKindDef.Named("BI_Spelopede_Green") };
        public static PawnKindDef[] megascarabKindDef = new PawnKindDef[] { PawnKindDef.Named("BI_Megascarab_Brown"), PawnKindDef.Named("BI_Megascarab_Red"), PawnKindDef.Named("BI_Megascarab_Black"), PawnKindDef.Named("BI_Megascarab_Green") };

        public static List<List<PawnKindDef>> spawnablePawnKinds = new List<List<PawnKindDef>>() { new List<PawnKindDef>() { PawnKindDef.Named("BI_Megaspider_Brown"), PawnKindDef.Named("BI_Spelopede_Brown"), PawnKindDef.Named("BI_Megascarab_Brown") },
            new List<PawnKindDef>() { PawnKindDef.Named("BI_Megaspider_Red"), PawnKindDef.Named("BI_Spelopede_Red"), PawnKindDef.Named("BI_Megascarab_Red") },
            new List<PawnKindDef>() { PawnKindDef.Named("BI_Megaspider_Black"), PawnKindDef.Named("BI_Spelopede_Black"), PawnKindDef.Named("BI_Megascarab_Black") },
            new List<PawnKindDef>() { PawnKindDef.Named("BI_Megaspider_Green"), PawnKindDef.Named("BI_Spelopede_Green"), PawnKindDef.Named("BI_Megascarab_Green") } };

        public static ThingDef ThingDefOfQueen = DefDatabase<ThingDef>.GetNamed("BI_Queen");
        public static ThingDef ThingDefOfEgg = DefDatabase<ThingDef>.GetNamed("BI_Egg");
        public static ThingDef ThingDefOfTunnelQueenSpawner = DefDatabase<ThingDef>.GetNamed("BI_TunnelQueenSpawner");
        public static ThingDef ThingDefOfTunnelMegascarabSpawner = DefDatabase<ThingDef>.GetNamed("BI_TunnelMegascarabSpawner");

        public static int TotalSpawnedInsectCount(Map map)
        {
            int count = 0;
            List<Thing> things = map.listerThings.AllThings;
            if (things != null && things.Any())
            {
                foreach (Thing thing in things)
                {
                    Pawn pawn = thing as Pawn;
                    if (pawn != null && !pawn.Dead)
                    {
                        if (thing.def.thingClass == typeof(Queen) || thing.def.thingClass == typeof(Insect) || thing.def.thingClass == typeof(Egg))
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }
        public static int TotalHiveInsectCount(Queen queen)
        {
            int result = 0;
            if (queen.spawnedInsects.Any())
            {
                result += queen.spawnedInsects.Count();
            }
            if (queen.spawnedEggs.Any())
            {
                result += queen.spawnedEggs.Count();
            }
            return result;
        }
        public static Faction GetRandomInsectFaction()
        {
            List<Faction> insectFactions = new List<Faction>();
            List<Faction> factions = Find.FactionManager.AllFactions as List<Faction>;
            if (factions != null && factions.Any())
            {
                foreach (Faction faction in factions)
                {
                    if (faction.def.defName == "BI_InsectBrownFaction" || faction.def.defName == "BI_InsectRedFaction" || faction.def.defName == "BI_InsectBlackFaction" || faction.def.defName == "BI_InsectGreenFaction")
                    {
                        insectFactions.Add(faction);
                    }
                }
                if (insectFactions != null && insectFactions.Any())
                {
                    return insectFactions.RandomElement();
                }
            }
            return null;
        }

        public static PawnKindDef GetFactionKindDef(PawnKindDef kindDef, Faction faction)
        {
            int factionID = 0;
            if (faction.def == FactionDef.Named("BI_InsectRedFaction"))
            {
                factionID = 1;
            }
            else if (faction.def == FactionDef.Named("BI_InsectBlackFaction"))
            {
                factionID = 2;
            }
            else if (faction.def == FactionDef.Named("BI_InsectGreenFaction"))
            {
                factionID = 3;
            }
            if (queenKindDef.Contains(kindDef))
            {
                return queenKindDef[factionID];
            }
            else if (megaspiderKindDef.Contains(kindDef))
            {
                return megaspiderKindDef[factionID];
            }
            else if (spelopedeKindDef.Contains(kindDef))
            {
                return spelopedeKindDef[factionID];
            }
            else if (megascarabKindDef.Contains(kindDef))
            {
                return megascarabKindDef[factionID];
            }
            return null;
        }

        public static List<PawnKindDef> GetFactionKindDefs(Faction faction)
        {
            int factionID = 0;
            if (faction.def == FactionDef.Named("BI_InsectRedFaction"))
            {
                factionID = 1;
            }
            else if (faction.def == FactionDef.Named("BI_InsectBlackFaction"))
            {
                factionID = 2;
            }
            else if (faction.def == FactionDef.Named("BI_InsectGreenFaction"))
            {
                factionID = 3;
            }
            return spawnablePawnKinds[factionID];
        }

        public static Faction GetInsectFaction(Pawn pawn)
        {
            List<Faction> factions = Find.FactionManager.AllFactions as List<Faction>;
            Faction insectBrownFaction = null;
            Faction insectRedFaction = null;
            Faction insectBlackFaction = null;
            Faction insectGreenFaction = null;
            foreach (Faction faction in factions)
            {
                if (faction.def.defName == "BI_InsectBrownFaction")
                    insectBrownFaction = faction;
                else if (faction.def.defName == "BI_InsectRedFaction")
                    insectRedFaction = faction;
                else if (faction.def.defName == "BI_InsectBlackFaction")
                    insectBlackFaction = faction;
                else if (faction.def.defName == "BI_InsectGreenFaction")
                    insectGreenFaction = faction;
            }
            if (insect_BrownKindDef.Contains(pawn.kindDef))
            {
                return insectBrownFaction;
            }
            else if (insect_RedKindDef.Contains(pawn.kindDef))
            {
                return insectRedFaction;
            }
            else if (insect_BlackKindDef.Contains(pawn.kindDef))
            {
                return insectBlackFaction;
            }
            else if (insect_GreenKindDef.Contains(pawn.kindDef))
            {
                return insectGreenFaction;
            }
            return null;
        }

        public static bool IsInsectFaction(Faction faction)
        {
            bool result = false;
            if (faction.def.defName == "BI_InsectBrownFaction" || faction.def.defName == "BI_InsectRedFaction" || faction.def.defName == "BI_InsectBlackFaction" || faction.def.defName == "BI_InsectGreenFaction")
                result = true;
            return result;     
        }
        public static List<Thing> ListQueens(Map map)
        {
            List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOfQueen);
            if (list == null || !list.Any())
            {
                return null;
            }
            return list;
        }

        public static Queen FindQueen(Pawn pawn)
        {
            List<Thing> things = ListQueens(pawn.Map);
            if (things != null && things.Any())
            {
                foreach (Thing thing in things)
                {
                    Queen queen = thing as Queen;
                    if (queen != null)
                    {
                        List<Pawn> list = queen.spawnedInsects;
                        if (list != null && list.Any())
                        {
                            foreach (Pawn pawn2 in list)
                            {
                                if (pawn2 != null)
                                {
                                    if (pawn == pawn2 || pawn == queen)
                                    {
                                        return queen;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static int HiveFoodCount(Queen queen, int maxDistance = 5)
        {
            if (queen != null)
            {
                List<Thing> things = queen.Map.listerThings.ThingsOfDef(ThingDefOf.InsectJelly);
                if (things != null && things.Any())
                {
                    int foodAmount = 0;
                    float dist = 0f;
                    foreach (Thing thing in things)
                    {
                        maxDistance = 12;
                        dist = IntVec3Utility.ManhattanDistanceFlat(thing.Position, queen.hiveLocation);
                        if (dist <= maxDistance)
                        {
                            foodAmount = foodAmount + thing.stackCount;
                            continue;
                        }
                        List<Egg> eggs = queen.spawnedEggs;
                        if (eggs != null && eggs.Any())
                        {
                            maxDistance = 5;
                            foreach (Egg egg in eggs)
                            {
                                if (egg != null)
                                {
                                    dist = IntVec3Utility.ManhattanDistanceFlat(thing.Position, egg.Position);
                                    if (dist <= maxDistance)
                                    {
                                        foodAmount = foodAmount + thing.stackCount;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    return foodAmount;
                }
            }
            return 0;
        }
        public static bool JobGivenRecentTick(Pawn pawn, string JobName)
        {
            List<string> jobs = HarmonyPatches.jobsGivenRecentTicksTextual(pawn.jobs);
            if (jobs != null && jobs.Any())
            {
                foreach (string job in jobs)
                {
                    if (job != null)
                    {
                        if (job.Contains(JobName))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}