using System;
using System.Linq;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace BetterInfestations
{
    public class BetterInfestationsSettings : ModSettings
    {
        public List<string> disabledDefList = new List<string>();
        public bool showNotifications = true;
        public int foodStorage = 250;
        public float hiveAggression = 0.5f;
        public float eggHatchingRate = 2f;
        public float eggLayingRate = 0.5f;
        public float hiveDistFromColony = 7f;
        public int maxInsectsHive = 40;
        public int maxInsectsMap = 160;
        public int maxQueens = 12;
        public bool allowHiveMapGen = false;
        public int maxMapGenHives = 3;
        public bool allowHarvestJob = true;
        public bool allowSapperJob = true;
        public bool allowHuntingJob = true;
        public bool spawnAdditionalInsects = true;
        public int maxParamPointsInf = 200;
        public int maxParamPointsDeep = 500;
        public bool allowHiveSpawnColony = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref disabledDefList, "disabledDefList");
            Scribe_Values.Look(ref showNotifications, "showNotifications", true);
            Scribe_Values.Look(ref foodStorage, "foodStorage", 250);
            Scribe_Values.Look(ref hiveAggression, "hiveAggression", 0.5f);
            Scribe_Values.Look(ref eggHatchingRate, "eggHatchingRate", 2f);
            Scribe_Values.Look(ref eggLayingRate, "eggLayingRate", 0.5f);
            Scribe_Values.Look(ref hiveDistFromColony, "hiveDistFromColony", 7f);
            Scribe_Values.Look(ref maxInsectsHive, "maxInsectsHive", 40);
            Scribe_Values.Look(ref maxInsectsMap, "maxInsectsMap", 160);
            Scribe_Values.Look(ref maxQueens, "maxQueens", 12);
            Scribe_Values.Look(ref allowHiveMapGen, "allowHiveMapGen", false);
            Scribe_Values.Look(ref maxMapGenHives, "maxMapGenHives", 3);
            Scribe_Values.Look(ref allowHarvestJob, "allowHarvestJob", true);
            Scribe_Values.Look(ref allowSapperJob, "allowSapperJob", true);
            Scribe_Values.Look(ref allowHuntingJob, "allowHuntingJob", true);
            Scribe_Values.Look(ref spawnAdditionalInsects, "spawnAdditionalInsects", true);
            Scribe_Values.Look(ref maxParamPointsInf, "maxParamPoints", 200);
            Scribe_Values.Look(ref maxParamPointsDeep, "maxParamPoints", 500);
            Scribe_Values.Look(ref allowHiveSpawnColony, "allowHiveSpawnColony", true);
        }
    }

    [StaticConstructorOnStartup]
    public class BetterInfestationsMod : Mod
    {
        public static BetterInfestationsSettings settings;
        public bool menuSettings = false;
        Vector2 leftScrollPosition;
        Vector2 rightScrollPosition;
        string searchTerm = string.Empty;
        ThingDef leftSelectedDef;
        ThingDef rightSelectedDef;

        public BetterInfestationsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<BetterInfestationsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            if (menuSettings)
            {
                Rect topRect = inRect.TopPart(0.05f);
                if (Widgets.ButtonText(topRect.RightPart(0.95f).LeftPart(0.18f), "settings_settings_desc".Translate()))
                {
                    menuSettings = false;
                }
                topRect.y += 32;
                Text.Font = GameFont.Medium;
                if (searchTerm.NullOrEmpty())
                {
                    GUI.color = Color.gray;
                    Widgets.Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_searchbyname_desc".Translate());
                    GUI.color = Color.white;
                }
                searchTerm = Widgets.TextField(topRect.RightPart(0.95f).LeftPart(0.95f), searchTerm);
                Rect labelRect = inRect.TopPart(0.1f).BottomHalf();
                Rect bottomRect = inRect.BottomPart(0.9f);
                topRect.y += 32;
                Widgets.Label(topRect.RightPart(0.95f).LeftPart(0.425f), "settings_labelleft_desc".Translate());
                Widgets.Label(topRect.RightPart(0.45f).LeftPart(0.95f), "settings_labelright_desc".Translate());
                bottomRect.y += 32;
                bottomRect.height -= 32;

                List<ThingDef> disabledDefList = new List<ThingDef>();
                if (settings.disabledDefList != null && settings.disabledDefList.Any())
                {
                    disabledDefList = StringToDefList(settings.disabledDefList);
                }

                #region leftSide
                List<ThingDef> leftList = new List<ThingDef>();
                Rect leftRect = bottomRect.LeftHalf().RightPart(0.9f).LeftPart(0.9f);
                GUI.BeginGroup(leftRect, new GUIStyle(GUI.skin.box));
                List<ThingDef> found = new List<ThingDef>();
                found = DefDatabase<ThingDef>.AllDefs.Where(x =>
                FilteredTarget(x) && !settings.disabledDefList.Contains(x.defName)).OrderBy(x => x.defName).ToList();
                float num = 3f;
                Widgets.BeginScrollView(leftRect.AtZero(), ref leftScrollPosition, new Rect(0f, 0f, leftRect.width / 10 * 9, found.Count() * 32f));
                if (!found.NullOrEmpty())
                {
                    foreach (ThingDef def in found)
                    {
                        if ((!searchTerm.NullOrEmpty() && def.defName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) || searchTerm.NullOrEmpty())
                        {
                            leftList.Add(def);
                            Rect rowRect = new Rect(5, num, leftRect.width - 6, 30);
                            Widgets.DrawHighlightIfMouseover(rowRect);
                            if (def == leftSelectedDef)
                                Widgets.DrawHighlightSelected(rowRect);
                            Widgets.Label(rowRect, def.defName);
                            if (Widgets.ButtonInvisible(rowRect))
                            {
                                leftSelectedDef = def;
                                rightSelectedDef = null;
                            }
                            num += 32f;
                        }
                    }
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
                #endregion

                #region rightSide
                List<ThingDef> rightList = new List<ThingDef>();
                Rect rightRect = bottomRect.RightHalf().RightPart(0.9f).LeftPart(0.9f);
                GUI.BeginGroup(rightRect, GUI.skin.box);
                num = 6f;
                Widgets.BeginScrollView(rightRect.AtZero(), ref rightScrollPosition, new Rect(0f, 0f, rightRect.width / 5 * 4, disabledDefList.Count * 32f));
                if (!disabledDefList.NullOrEmpty())
                {
                    foreach (ThingDef def in disabledDefList)
                    {
                        if ((!searchTerm.NullOrEmpty() && def.defName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) || searchTerm.NullOrEmpty())
                        {
                            rightList.Add(def);
                            Rect rowRect = new Rect(5, num, leftRect.width - 6, 30);
                            Widgets.DrawHighlightIfMouseover(rowRect);
                            if (def == rightSelectedDef)
                                Widgets.DrawHighlightSelected(rowRect);
                            Widgets.Label(rowRect, def.defName);
                            if (Widgets.ButtonInvisible(rowRect))
                            {
                                rightSelectedDef = def;
                                leftSelectedDef = null;
                            }
                            num += 32f;
                        }
                    }
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
                #endregion


                #region buttons
                if (Widgets.ButtonImage(bottomRect.BottomPart(0.67f).TopPart(0.17f).RightPart(0.525f).LeftPart(0.1f), TexUI.ArrowTexRight) && leftSelectedDef != null)
                {
                    settings.disabledDefList.Add(leftSelectedDef.defName);
                    settings.disabledDefList = settings.disabledDefList.OrderBy(x => x).ToList();
                    bool leftSelectChanged = false;
                    int leftListCount = leftList.Count;
                    if (leftListCount > 0)
                    {
                        for (int i = 0; i < leftListCount; i++)
                        {
                            if (leftList[i] == DefDatabase<ThingDef>.GetNamed(leftSelectedDef.defName))
                            {
                                if ((leftListCount - 1) > i)
                                {
                                    leftSelectedDef = leftList[i+1];
                                    leftSelectChanged = true;
                                    break;
                                }
                            }
                        }
                    }
                    leftList.Clear();
                    if (!leftSelectChanged)
                    {
                        leftSelectedDef = null;
                    }
                }
                if (Widgets.ButtonImage(bottomRect.BottomPart(0.47f).TopPart(0.23f).RightPart(0.525f).LeftPart(0.1f), TexUI.ArrowTexLeft) && rightSelectedDef != null)
                {
                    settings.disabledDefList.Remove(rightSelectedDef.defName);
                    bool rightSelectChanged = false;
                    int rightListCount = rightList.Count;
                    if (rightListCount > 0)
                    {
                        for (int i = 0; i < rightListCount; i++)
                        {
                            if (rightList[i] == DefDatabase<ThingDef>.GetNamed(rightSelectedDef.defName))
                            {
                                if ((rightListCount - 1) > i)
                                {
                                    rightSelectedDef = rightList[i+1];
                                    rightSelectChanged = true;
                                    break;
                                }
                            }
                        }
                    }
                    rightList.Clear();
                    if (!rightSelectChanged)
                    {
                        rightSelectedDef = null;
                    }
                }
                #endregion

                settings.Write();
            }
            else
            {
                Rect topRect = inRect.TopPart(0.05f);
                if (ButtonText(topRect.RightPart(0.95f).LeftPart(0.18f), "settings_targeting_desc".Translate()))
                {
                    menuSettings = true;
                }
                topRect.y += 72;

                CheckboxLabeled(topRect.RightPart(0.95f).LeftPart(0.425f), "settings_notification_desc".Translate(), ref settings.showNotifications, "settings_notification_tip".Translate());
                CheckboxLabeled(topRect.RightPart(0.475f).LeftPart(0.9f), "settings_allowharvestjob_desc".Translate(), ref settings.allowHarvestJob, "settings_allowharvestjob_tip".Translate());
                topRect.y += 32;

                CheckboxLabeled(topRect.RightPart(0.95f).LeftPart(0.425f), "settings_allowhivemapgen_desc".Translate(), ref settings.allowHiveMapGen, "settings_allowhivemapgen_tip".Translate());
                CheckboxLabeled(topRect.RightPart(0.475f).LeftPart(0.9f), "settings_allowsapperjob_desc".Translate(), ref settings.allowSapperJob, "settings_allowsapperjob_tip".Translate());
                topRect.y += 32;

                CheckboxLabeled(topRect.RightPart(0.95f).LeftPart(0.425f), "settings_allowhivespawncolony_desc".Translate(), ref settings.allowHiveSpawnColony, "settings_allowhivespawncolony_tip".Translate());
                CheckboxLabeled(topRect.RightPart(0.475f).LeftPart(0.9f), "settings_allowhuntingjob_desc".Translate(), ref settings.allowHuntingJob, "settings_allowhuntingjob_tip".Translate());
                topRect.y += 32;

                CheckboxLabeled(topRect.RightPart(0.95f).LeftPart(0.425f), "settings_spawnadditionalinsects_desc".Translate(), ref settings.spawnAdditionalInsects, "settings_spawnadditionalinsects_tip".Translate());
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_maxparampointsinf_desc".Translate() + settings.maxParamPointsInf, "settings_maxparampointsinf_tip".Translate());
                settings.maxParamPointsInf = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.maxParamPointsInf, 200f, 1000f, 50f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_maxparampointsdeep_desc".Translate() + settings.maxParamPointsDeep, "settings_maxparampointsdeep_tip".Translate());
                settings.maxParamPointsDeep = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.maxParamPointsDeep, 200f, 1000f, 50f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_maxmapgenhives_desc".Translate() + settings.maxMapGenHives, "settings_maxmapgenhives_tip".Translate());
                settings.maxMapGenHives = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.maxMapGenHives, 1f, 5f, 1f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_maxinsectsmap_desc".Translate() + settings.maxInsectsMap, "settings_maxinsectsmap_tip".Translate());
                settings.maxInsectsMap = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.maxInsectsMap, 40f, 400f, 10f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_maxinsectshive_desc".Translate() + settings.maxInsectsHive, "settings_maxinsectshive_tip".Translate());
                settings.maxInsectsHive = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.maxInsectsHive, 10f, 100f, 10f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_maxqueens_desc".Translate() + settings.maxQueens, "settings_maxqueens_tip".Translate());
                settings.maxQueens = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.maxQueens, 1f, 20f, 1f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_hiveaggression_desc".Translate() + (int)Math.Round(settings.hiveAggression * 100f) + "%", "settings_hiveaggression_tip".Translate());
                settings.hiveAggression = HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.hiveAggression, 0f, 1f, 0.1f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_hivedistance_desc".Translate() + settings.hiveDistFromColony, "settings_hivedistance_tip".Translate());
                settings.hiveDistFromColony = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.hiveDistFromColony, 1f, 10f, 1f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_foodstorage_desc".Translate() + settings.foodStorage, "settings_foodstorage_tip".Translate());
                settings.foodStorage = (int)HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.foodStorage, 0f, 500f, 50f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_egglayingrate_desc".Translate() + settings.eggLayingRate + "settings_days_desc".Translate(), "settings_egglayingrate_tip".Translate());
                settings.eggLayingRate = HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.eggLayingRate, 0f, 10f, 0.25f);
                topRect.y += 32;

                Label(topRect.RightPart(0.95f).LeftPart(0.95f), "settings_egghatchingrate_desc".Translate() + settings.eggHatchingRate + "settings_days_desc".Translate(), "settings_egghatchingrate_tip".Translate());
                settings.eggHatchingRate = HorizontalSlider(topRect.RightPart(0.575f).LeftPart(0.9f), settings.eggHatchingRate, 0f, 10f, 0.25f);
            }
        }
        public static List<ThingDef> StringToDefList(List<string> list)
        {
            List<ThingDef> defs = new List<ThingDef>();
            if (list != null && list.Any())
            {
                foreach (string str in list)
                {
                    defs.Add(DefDatabase<ThingDef>.GetNamed(str));
                }
                return defs;
            }
            return null;
        }

        public static bool FilteredTarget(ThingDef def)
        {
            if (def.category.ToString() == "Pawn" && def.race.IsFlesh)
            {
                return true;
            }
            else if (def.category.ToString() == "Item" && !def.IsCorpse && (def.IsIngestible || def.defName == "WoodLog"))
            {
                return true;
            }
            else if (def.category.ToString() == "Plant" && def.plant.Harvestable && def.plant.harvestedThingDef != null && (def.plant.harvestedThingDef.IsIngestible || def.plant.harvestedThingDef.defName == "WoodLog"))
            {
                return true;
            }
            return false;
        }

        public static bool ButtonText(Rect rect, string label)
        {
            bool result = Widgets.ButtonText(rect, label, true, false, true);
            return result;
        }

        public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, string tooltip)
        {
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            TooltipHandler.TipRegion(rect, tooltip);
            Widgets.CheckboxLabeled(rect, label, ref checkOn, false, null, null, false);
        }

        public static void Label(Rect rect, string label, string tooltip)
        {
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            TooltipHandler.TipRegion(rect, tooltip);
            Widgets.Label(rect, label);
        }

        public static float HorizontalSlider(Rect rect, float val, float min, float max, float roundTo)
        {
            float result = Widgets.HorizontalSlider(rect, val, min, max, false, null, null, null, roundTo);
            return result;
        }

        public override string SettingsCategory() => "Better Infestations";
    }
}