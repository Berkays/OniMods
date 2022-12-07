using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.Core;
using KMod;

namespace SweepZones
{
    public class SweepZonePatches : KMod.UserMod2
    {
        [PLibMethod(RunAt.BeforeDbInit)]
        internal static void BeforeDbInit()
        {
            var toolSetIcon = SweepZones.ICONS.TOOL_ICON_SPRITE;
            var setVisualizerIcon = SweepZones.ICONS.SET_VISUALIZER_SPRITE;
            var cancelVisualizerIcon = SweepZones.ICONS.CANCEL_VISUALIZER_SPRITE;
            Assets.Sprites.Add(toolSetIcon.name, toolSetIcon);
            Assets.Sprites.Add(setVisualizerIcon.name, setVisualizerIcon);
            Assets.Sprites.Add(cancelVisualizerIcon.name, cancelVisualizerIcon);
        }
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new PPatchManager(harmony).RegisterPatchClass(typeof(SweepZonePatches));
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);
            ModIntegrations.LoadIntegrations();
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch(nameof(Db.Initialize))]
        static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Strings.Add(UI.STRINGS.OVERLAY_NAME.Key, UI.STRINGS.OVERLAY_NAME.Value);
                Strings.Add(UI.STRINGS.OVERLAY_DESCRIPTION.Key, UI.STRINGS.OVERLAY_DESCRIPTION.Value);
            }
        }

        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        static class PlayerControllerOnPrefabInit
        {
            public static void Postfix(PlayerController __instance)
            {
                List<InterfaceTool> interfaceTools = new List<InterfaceTool>(__instance.tools);

                GameObject logicObj = new GameObject("SweepZoneLogic");
                logicObj.AddOrGet<SweepZoneLogic>();
                logicObj.gameObject.SetActive(true);

                GameObject toolObj = new GameObject("SweepZoneTool");
                toolObj.AddOrGet<SweepZoneTool>();

                toolObj.transform.SetParent(__instance.gameObject.transform);
                toolObj.gameObject.SetActive(true);
                toolObj.gameObject.SetActive(false);

                interfaceTools.Add(toolObj.GetComponent<InterfaceTool>());

                __instance.tools = interfaceTools.ToArray();
            }
        }

        [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
        class SaveGame_OnPrefabInit_Patch
        {
            public static void Postfix(SaveGame __instance)
            {
                __instance.gameObject.AddOrGet<SaveState>();
            }
        }

        [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
        static class ToolMenuCreateBasicTools
        {
            public static void Prefix(ToolMenu __instance)
            {
                __instance.basicTools.Add(ToolMenu.CreateToolCollection(
                    SweepZones.UI.STRINGS.TOOL_TITLE,
                    SweepZones.UI.STRINGS.TOOL_ICON,
                    Action.Overlay10,
                    nameof(SweepZones.SweepZoneTool),
                    SweepZones.UI.STRINGS.TOOL_DESCRIPTION,
                    false
                ));
            }
        }

        [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
        static class OverlayLegend_OnSpawn_Patch
        {
            public static void Prefix(OverlayLegend __instance)
            {
                var instance = Traverse.Create(__instance);

                if (instance.Field("overlayInfoList").FieldExists() && instance.Field("overlayInfoList").GetValue<List<OverlayLegend.OverlayInfo>>() != null)
                {
                    var info = new OverlayLegend.OverlayInfo
                    {
                        name = UI.STRINGS.OVERLAY_NAME.Key,
                        mode = SweepZoneOverlay.ID,
                        infoUnits = new List<OverlayLegend.OverlayInfoUnit>(),
                        isProgrammaticallyPopulated = true
                    };

                    instance.Field("overlayInfoList").GetValue<List<OverlayLegend.OverlayInfo>>().Add(info);
                }
            }
        }

        [HarmonyPatch(typeof(OverlayMenu), "InitializeToggles")]
        static class OverlayMenu_InitializeToggles_Patch
        {
            public static void Postfix(List<KIconToggleMenu.ToggleInfo> ___overlayToggleInfos)
            {
                var constructor = AccessTools.Constructor(
                    AccessTools.Inner(typeof(OverlayMenu), "OverlayToggleInfo"),
                    new[] {
                        typeof(string),
                        typeof(string),
                        typeof(HashedString),
                        typeof(string),
                        typeof(Action),
                        typeof(string),
                        typeof(string)
                    }
                );

                var obj = constructor.Invoke(
                    new object[] {
                        SweepZones.UI.STRINGS.OVERLAY_NAME.Value,
                        SweepZones.UI.STRINGS.OVERLAY_ICON,
                        SweepZoneOverlay.ID,
                        "",
                        Action.NumActions,
                        "",
                        SweepZones.UI.STRINGS.OVERLAY_NAME.Value
                    }
                );

                ___overlayToggleInfos.Add((KIconToggleMenu.ToggleInfo)obj);
            }
        }

        [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
        static class OverlayScreen_RegisterModes_Patch
        {
            private delegate void RegisterModeDelegate(OverlayScreen instance, OverlayModes.Mode mode);
            private static readonly RegisterModeDelegate RegisterMode = (RegisterModeDelegate)typeof(OverlayScreen).GetMethod("RegisterMode", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(RegisterModeDelegate));

            internal static void Postfix(OverlayScreen __instance)
            {
                RegisterMode(__instance, new SweepZoneOverlay());
            }
        }

        [HarmonyPatch(typeof(StatusItem), "GetStatusItemOverlayBySimViewMode")]
        static class StatusItem_GetStatusItemOverlayBySimViewMode_Patch
        {
            public static void Prefix(Dictionary<HashedString, StatusItem.StatusItemOverlays> ___overlayBitfieldMap)
            {
                if (!___overlayBitfieldMap.ContainsKey(SweepZoneOverlay.ID))
                    ___overlayBitfieldMap.Add(SweepZoneOverlay.ID, StatusItem.StatusItemOverlays.None);

            }
        }

        [HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
        static class SimDebugView_OnPrefabInit_Patch
        {
            internal static void Postfix(IDictionary<HashedString, Func<SimDebugView, int, Color>> ___getColourFuncs)
            {
                ___getColourFuncs.Add(SweepZoneOverlay.ID, SweepZoneOverlay.GetColor);
            }
        }

        [HarmonyPatch(typeof(ToolMenu), "OnPrefabInit")]
        static class ToolMenu_OnPrefabInit_Patch
        {
            internal static void Postfix()
            {
                SweepToolMenu.CreateInstance();
            }
        }
    }
}
