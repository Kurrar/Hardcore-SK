using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private const BindingFlags allFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

        public static FieldInfo _jobsGivenRecentTicksTextual;
        public static FieldInfo _cachedVerbProperties;
        public static FieldInfo _pawnPawnNativeVerbs;
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("betterinfestations.harmony");
            {
                Type type = typeof(Pawn_JobTracker);
                _jobsGivenRecentTicksTextual = type.GetField("jobsGivenRecentTicksTextual", allFlags);
            }
            {
                Type type = typeof(Pawn_NativeVerbs);
                _cachedVerbProperties = type.GetField("cachedVerbProperties", allFlags);
                _pawnPawnNativeVerbs = type.GetField("pawn", allFlags);
                harmony.Patch(type.GetMethod("CheckCreateVerbProperties", allFlags), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_CheckCreateVerbProperties))), null);
            }
            {
                Type type = typeof(PathFinder);
                harmony.Patch(type.GetMethod("GetBuildingCost", allFlags), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_GetBuildingCost))), null);
            }
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        public static List<string> jobsGivenRecentTicksTextual(Pawn_JobTracker instance)
        {
            return (List<string>)_jobsGivenRecentTicksTextual.GetValue(instance);
        }
        public static Pawn pawnPawnNativeVerbs(Pawn_NativeVerbs instance)
        {
            return (Pawn)_pawnPawnNativeVerbs.GetValue(instance);
        }
        public static List<VerbProperties> cachedVerbProperties(Pawn_NativeVerbs instance)
        {
            return (List<VerbProperties>)_cachedVerbProperties.GetValue(instance);
        }
        public static bool Patch_CheckCreateVerbProperties(ref Pawn_NativeVerbs __instance)
        {
            if (_cachedVerbProperties.GetValue(__instance) != null)
            {
                return true;
            }
            if (pawnPawnNativeVerbs(__instance).def.thingClass == typeof(Insect) || pawnPawnNativeVerbs(__instance).def.thingClass == typeof(Queen))
            {
                _cachedVerbProperties.SetValue(__instance, new List<VerbProperties>());
                cachedVerbProperties(__instance).Add(NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.BeatFire));
                return false;
            }
            return true;
        }
        public static bool Patch_GetBuildingCost(ref int __result, ref Building b, ref TraverseParms traverseParms, ref Pawn pawn)
        {
            if (pawn != null)
            {
                Insect insect = pawn as Insect;
                if (insect != null && traverseParms != null && traverseParms.mode == TraverseMode.PassAllDestroyableThings)
                {
                    if (b != null && b.def != null && b.def.passability == Traversability.Impassable && b.def.size == IntVec2.One && b.Faction != null && b.Faction.IsPlayer)
                    {
                        __result = 0;
                    }
                    else
                    {
                        __result = 5000;
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
