using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using PeterHan.PLib.Core;
using Database;

namespace AutomationBypass
{
    internal class AutomationBypassPatches
    {
        static readonly Color32 bypassedColor = new Color32(237, 158, 65, 255);
        internal static StatusItem bypassStatusItem = null;

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch(nameof(Db.Initialize))]
        static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Strings.Add($"STRINGS.UI.OVERLAYS.LOGIC.BYPASSED", UI.OVERLAYS.LOGIC.TOOLTIPS.BYPASSED);
                Strings.Add($"STRINGS.UI.OVERLAYS.LOGIC.TOOLTIPS.BYPASSED", UI.OVERLAYS.LOGIC.TOOLTIPS.BYPASSED);
            }
        }

        [HarmonyPatch(typeof(MiscStatusItems), "CreateStatusItems")]
        static class MiscStatusItems_CreateStatusItems_Patch
        {
            internal static void Postfix(MiscStatusItems __instance)
            {
                var methodInfo = AccessTools.Method(typeof(MiscStatusItems), "CreateStatusItem", new Type[] {typeof(string), typeof(string), typeof(string),
                    typeof(StatusItem.IconType), typeof(NotificationType),
                    typeof(bool), typeof(HashedString), typeof(bool), typeof(int) });

                bypassStatusItem = (StatusItem)methodInfo.Invoke(__instance, new object[] { "Bypassed",
                                "MISC", "status_item_no_logic_wire_connected", StatusItem.IconType.Custom,
                                NotificationType.Neutral, false, OverlayModes.None.ID, true, 129022});
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch(nameof(GeneratedBuildings.InitializeLogicPorts))]
        static class GeneratedBuildings_InitializeLogicPorts_Patch
        {
            internal static void Postfix(GameObject go, BuildingDef def)
            {
                if (go.TryGetComponent<LogicPorts>(out LogicPorts lp))
                {
                    if (lp.inputPortInfo != null && lp.inputPortInfo.Length > 0)
                        go.AddOrGet<ToggleButton>();
                }
            }
        }

        [HarmonyPatch(typeof(LogicOperationalController), "CheckWireState")]
        static class LogicOperationalController_CheckWireState_Patch
        {
            internal static bool Prefix(LogicOperationalController __instance)
            {
                if (__instance.gameObject.TryGetComponent(out ToggleButton comp) && comp.IsEnabled)
                {
                    __instance.operational.SetFlag(LogicOperationalController.LogicOperationalFlag, true);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(OverlayModes.Logic))]
        [HarmonyPatch(nameof(OverlayModes.Logic.GetCustomLegendData))]
        static class OverlayModes_Logic_GetCustomLegendData_Patch
        {
            internal static void Postfix(ref List<LegendEntry> __result)
            {
                __result.Add(new LegendEntry(UI.OVERLAYS.LOGIC.BYPASSED, UI.OVERLAYS.LOGIC.TOOLTIPS.BYPASSED, bypassedColor));
            }
        }

        [HarmonyPatch(typeof(OverlayModes.Logic))]
        [HarmonyPatch("UpdateUI")]
        static class OverlayModes_UpdateUI_Patch
        {
            static readonly Type uiInfoType = PPatchTools.GetTypeSafe("OverlayModes+Logic+UIInfo");
            static readonly FieldInfo bitDepthField = uiInfoType.GetField("bitDepth", BindingFlags.Instance | BindingFlags.Public);
            static readonly FieldInfo imageField = uiInfoType.GetField("image", BindingFlags.Instance | BindingFlags.Public);
            static readonly FieldInfo cellField = uiInfoType.GetField("cell", BindingFlags.Instance | BindingFlags.Public);

            static readonly Sprite logicInput = Assets.GetSprite("logicInput");
            static readonly Color32 logicOn = new Color32(GlobalAssets.Instance.colorSet.logicOn.r, GlobalAssets.Instance.colorSet.logicOn.g, GlobalAssets.Instance.colorSet.logicOn.b, byte.MaxValue);
            static readonly Color32 logicOff = new Color32(GlobalAssets.Instance.colorSet.logicOff.r, GlobalAssets.Instance.colorSet.logicOff.g, GlobalAssets.Instance.colorSet.logicOff.b, byte.MaxValue);

            internal static void Postfix(KCompactedVector<object> ___uiInfo)
            {
                foreach (object data in ___uiInfo)
                {
                    if (data == null)
                        continue;

                    int bitDepth = (int)bitDepthField.GetValue(data);
                    if (bitDepth == 1)
                    {
                        Image img = imageField.GetValue(data) as Image;

                        if (img != null)
                        {
                            int cell = (int)cellField.GetValue(data);
                            var obj = Grid.Objects[cell, (int)ObjectLayer.Building];

                            if (obj != null && obj.TryGetComponent<Building>(out Building buildingComp))
                            {
                                int auxport = cell;
                                if (obj.TryGetComponent<LogicPorts>(out LogicPorts lp))
                                {
                                    if (lp.inputPorts != null && lp.inputPorts.Count > 0)
                                        auxport = lp.inputPorts[0].GetLogicUICell();
                                }

                                if (cell != auxport)
                                    continue;

                                if (obj.TryGetComponent<ToggleButton>(out ToggleButton btn))
                                {

                                    if (btn.IsEnabled)
                                    {
                                        img.color = bypassedColor;
                                        img.sprite = AutomationBypass.ICONS.BYPASSED_PORT_SPRITE;
                                        continue;
                                    }

                                    img.sprite = logicInput;
                                    LogicCircuitNetwork networkForCell = Game.Instance.logicCircuitManager.GetNetworkForCell(cell);
                                    if (networkForCell != null)
                                    {
                                        img.color = networkForCell.IsBitActive(0) ? logicOn : logicOff;
                                        continue;
                                    }

                                    img.color = GlobalAssets.Instance.colorSet.logicDisconnected;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}