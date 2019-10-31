using System;
using System.Collections;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public static class HarvestHelper
    {
        public static Plant ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Plant> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionsMax = -1, bool forceGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
        {
            bool flag = searchRegionsMax < 0 || forceGlobalSearch;
            if (!flag && customGlobalSearchSet != null)
            {
                Log.ErrorOnce("searchRegionsMax >= 0 && customGlobalSearchSet != null && !forceGlobalSearch. customGlobalSearchSet will never be used.", 634984, false);
            }
            if (!flag && !thingReq.IsUndefined && !thingReq.CanBeFoundInRegion)
            {
                Log.ErrorOnce("ClosestThingReachable with thing request group " + thingReq.group + " and global search not allowed. This will never find anything because this group is never stored in regions. Either allow global search or don't call this method at all.", 518498981, false);
                return null;
            }
            if (EarlyOutSearch(root, map, thingReq, customGlobalSearchSet))
            {
                return null;
            }
            Plant plant = null;
            bool flag2 = false;
            if (!thingReq.IsUndefined && thingReq.CanBeFoundInRegion)
            {
                int num = (searchRegionsMax <= 0) ? 30 : searchRegionsMax;
                int num2;
                plant = RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, null, searchRegionsMin, num, maxDistance, out num2, traversableRegionTypes, ignoreEntirelyForbiddenRegions);
                flag2 = (plant == null && num2 < num);
            }
            if (plant == null && flag && !flag2)
            {
                if (traversableRegionTypes != RegionType.Set_Passable)
                {
                    Log.ErrorOnce("ClosestThingReachable had to do a global search, but traversableRegionTypes is not set to passable only. It's not supported, because Reachability is based on passable regions only.", 14384767, false);
                }
                Predicate<Plant> validator2 = (Plant p) => map.reachability.CanReach(root, p, peMode, traverseParams) && (validator == null || validator(p));
                IEnumerable<Thing> searchSet = customGlobalSearchSet ?? map.listerThings.ThingsMatching(thingReq);
                plant = ClosestThing_Global(root, searchSet, maxDistance, validator2, null);
            }
            return plant;
        }

        private static bool EarlyOutSearch(IntVec3 start, Map map, ThingRequest thingReq, IEnumerable<Thing> customGlobalSearchSet)
        {
            if (thingReq.group == ThingRequestGroup.Everything)
            {
                Log.Error("Cannot do ClosestThingReachable searching everything without restriction.", false);
                return true;
            }
            if (!start.InBounds(map))
            {
                Log.Error(string.Concat(new object[]
                {
            "Did FindClosestThing with start out of bounds (",
            start,
            "), thingReq=",
            thingReq
                }), false);
                return true;
            }
            return thingReq.group == ThingRequestGroup.Nothing || (customGlobalSearchSet == null && !thingReq.IsUndefined && map.listerThings.ThingsMatching(thingReq).Count == 0);
        }

        public static Plant RegionwiseBFSWorker(IntVec3 root, Map map, ThingRequest req, PathEndMode peMode, TraverseParms traverseParams, Predicate<Plant> validator, Func<Thing, float> priorityGetter, int minRegions, int maxRegions, float maxDistance, out int regionsSeen, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
        {
            regionsSeen = 0;
            if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
            {
                Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThings. Use ClosestThingGlobal.", false);
                return null;
            }
            if (traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater)
            {
                Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThingsNotWater. Use ClosestThingGlobal.", false);
                return null;
            }
            if (!req.IsUndefined && !req.CanBeFoundInRegion)
            {
                Log.ErrorOnce("RegionwiseBFSWorker with thing request group " + req.group + ". This group is never stored in regions. Most likely a global search should have been used.", 385766189, false);
                return null;
            }
            Region region = root.GetRegion(map, traversableRegionTypes);
            if (region == null)
            {
                return null;
            }
            float maxDistSquared = maxDistance * maxDistance;
            RegionEntryPredicate entryCondition = (Region from, Region to) => to.Allows(traverseParams, false) && (maxDistance > 5000f || to.extentsClose.ClosestDistSquaredTo(root) < maxDistSquared);
            Plant closestPlant = null;
            float closestDistSquared = 9999999f;
            float bestPrio = -3.40282347E+38f;
            int regionsSeenScan = 0;
            RegionProcessor regionProcessor = delegate (Region r)
            {
                if (RegionTraverser.ShouldCountRegion(r))
                {
                    regionsSeenScan++;
                }
                if (!r.IsDoorway && !r.Allows(traverseParams, true))
                {
                    return false;
                }
                if (!ignoreEntirelyForbiddenRegions || !r.IsForbiddenEntirely(traverseParams.pawn))
                {
                    List<Thing> list = r.ListerThings.ThingsMatching(req);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Plant plant = list[i] as Plant;
                        if (ReachabilityWithinRegion.ThingFromRegionListerReachable(plant, r, peMode, traverseParams.pawn))
                        {
                            float num = (priorityGetter == null) ? 0f : priorityGetter(plant);
                            if (num >= bestPrio)
                            {
                                float num2 = (plant.Position - root).LengthHorizontalSquared;
                                if ((num > bestPrio || num2 < closestDistSquared) && num2 < maxDistSquared && (validator == null || validator(plant)))
                                {
                                    closestPlant = plant;
                                    closestDistSquared = num2;
                                    bestPrio = num;
                                }
                            }
                        }
                    }
                }
                return regionsSeenScan >= minRegions && closestPlant != null;
            };
            RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
            regionsSeen = regionsSeenScan;
            return closestPlant;
        }

        public static Plant ClosestThing_Global(IntVec3 center, IEnumerable searchSet, float maxDistance = 99999f, Predicate<Plant> validator = null, Func<Thing, float> priorityGetter = null)
        {
            if (searchSet == null)
            {
                return null;
            }
            float num = 2.14748365E+09f;
            Plant result = null;
            float num2 = -3.40282347E+38f;
            float num3 = maxDistance * maxDistance;
            foreach (Plant p in searchSet)
            {
                if (p.Spawned)
                {
                    float num4 = (center - p.Position).LengthHorizontalSquared;
                    if (num4 <= num3)
                    {
                        if (priorityGetter != null || num4 < num)
                        {
                            if (validator == null || validator(p))
                            {
                                float num5 = 0f;
                                if (priorityGetter != null)
                                {
                                    num5 = priorityGetter(p);
                                    if (num5 < num2)
                                    {
                                        continue;
                                    }
                                    if (num5 == num2 && num4 >= num)
                                    {
                                        continue;
                                    }
                                }
                                result = p;
                                num = num4;
                                num2 = num5;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
