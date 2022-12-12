using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SweepZones
{
    public class SweepZoneOverlay : OverlayModes.Mode
    {
        private class OverlayEntry
        {
            internal readonly SaveState.State state;
            internal readonly List<LegendEntry> legendEntries;
            internal readonly ToolMode assignedTools;
            internal readonly ToolMode defaultTool;

            internal OverlayEntry(SaveState.State state, List<LegendEntry> legendEntries, ToolMode assignedTools, ToolMode defaultTool)
            {
                this.state = state;
                this.legendEntries = legendEntries;
                this.assignedTools = assignedTools;
                this.defaultTool = defaultTool;
            }
        }

        public static readonly HashedString ID = new HashedString("SWEEPZONES");

        private static readonly List<LegendEntry> sweepLegend = new List<LegendEntry>();
        private static readonly List<LegendEntry> mopLegend = new List<LegendEntry>();

        delegate void ChangeToSettingDelegate(ToolParameterMenu instance, string key);
        delegate void OnChangeDelegate(ToolParameterMenu instance);

        private readonly FieldInfo filterMenuFieldInfo = typeof(OverlayLegend).GetField("filterMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly ChangeToSettingDelegate changeToSetting = (ChangeToSettingDelegate)typeof(ToolParameterMenu).GetMethod("ChangeToSetting", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(ChangeToSettingDelegate));
        private readonly OnChangeDelegate onChange = (OnChangeDelegate)typeof(ToolParameterMenu).GetMethod("OnChange", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(OnChangeDelegate));

        private static Dictionary<string, OverlayEntry> overlayFilters;
        private static OverlayEntry activeOverlay;

        private static readonly int cameraLayerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");
        private static readonly int selectionMask = LayerMask.GetMask(new string[] {
                "MaskedOverlay"
        });

        static SweepZoneOverlay()
        {
            for (int i = 0; i < CommonProps.PRIORITY_COLORS.Length - 1; i++)
            {
                LegendEntry entry = new LegendEntry($"Priority {i + 1}", "", CommonProps.PRIORITY_COLORS[i]);
                sweepLegend.Add(entry);
                mopLegend.Add(entry);
            }

            if (ModIntegrations.ForbidItemsConfiguration.Enabled)
                sweepLegend.Add(new LegendEntry("Forbidden Zone", "", CommonProps.FORBID_COLOR));
        }

        public SweepZoneOverlay()
        {
            legendFilters = CreateDefaultFilters();
        }

        internal static void SetupOverlays()
        {
            overlayFilters = new Dictionary<string, OverlayEntry>() {
                { UI.STRINGS.OVERLAY_STATE_SWEEP.Key, new OverlayEntry(SaveState.Instance.Sweep, sweepLegend, ToolMode.Sweep | ToolMode.SweepClear | ToolMode.Forbid, ToolMode.Sweep) },
                { UI.STRINGS.OVERLAY_STATE_MOP.Key, new OverlayEntry(SaveState.Instance.Mop, mopLegend, ToolMode.Mop | ToolMode.MopClear, ToolMode.Mop) },
            };

            activeOverlay = overlayFilters.First().Value;
        }

        internal static Color GetColor(SimDebugView _, int cell)
        {
            if (!activeOverlay.state.ContainsCell(cell))
                return Color.black;

            return CommonProps.PRIORITY_COLORS[activeOverlay.state[cell].priority_value - 1];
        }

        public override void Disable()
        {
            UnregisterSaveLoadListeners();
            CameraController.Instance.ToggleColouredOverlayView(false);
            Camera.main.cullingMask &= ~cameraLayerMask;
            SelectTool.Instance.ClearLayerMask();
            base.Disable();

            var menu = SweepToolMenu.Instance;
            menu.OnSettingChanged -= ChangeFilter;
        }

        public override void Enable()
        {
            base.Enable();
            RegisterSaveLoadListeners();
            CameraController.Instance.ToggleColouredOverlayView(true);
            Camera.main.cullingMask |= cameraLayerMask;
            SelectTool.Instance.SetLayerMask(selectionMask);

            var menu = SweepToolMenu.Instance;
            menu.OnSettingChanged += ChangeFilter;
        }

        public override List<LegendEntry> GetCustomLegendData()
        {
            return activeOverlay.legendEntries;
        }

        public override Dictionary<string, ToolParameterMenu.ToggleState> CreateDefaultFilters()
        {
            return new Dictionary<string, ToolParameterMenu.ToggleState>
            {
                {
                    UI.STRINGS.OVERLAY_STATE_SWEEP.Key,
                    ToolParameterMenu.ToggleState.On
                },
                {
                    UI.STRINGS.OVERLAY_STATE_MOP.Key,
                    ToolParameterMenu.ToggleState.Off
                }
            };
        }

        // When overlay filter changed, switch to suitable tool
        public override void OnFiltersChanged()
        {
            foreach (var (legendFilter, state) in this.legendFilters)
            {
                if (state == ToolParameterMenu.ToggleState.On)
                {
                    activeOverlay = overlayFilters[legendFilter];

                    // Change to correct tool
                    SweepToolMenu menu = SweepToolMenu.Instance;

                    if (((int)menu.SelectedKey & (int)activeOverlay.assignedTools) == 0)
                    {
                        menu.OnSettingChanged -= ChangeFilter;
                        menu.SetOption(activeOverlay.defaultTool);
                        menu.OnSettingChanged += ChangeFilter;
                    }

                    return;
                }
            }

            activeOverlay = overlayFilters.First().Value;
        }

        // When tool option changed, switch to suitable overlay.
        private void ChangeFilter(ToolMode newToolMode)
        {
            var menu = (ToolParameterMenu)filterMenuFieldInfo.GetValue(OverlayLegend.Instance);

            // No change necessary
            if (((int)activeOverlay.assignedTools & (int)newToolMode) > 0)
                return;

            foreach (var (key, overlayEntry) in overlayFilters)
            {
                if (((int)overlayEntry.assignedTools & (int)newToolMode) > 0)
                {
                    // Set overlay filter
                    changeToSetting(menu, key);
                    onChange(menu);
                    return;
                }
            }
        }

        public override HashedString ViewMode()
        {
            return ID;
        }

        public override string GetSoundName()
        {
            return "Temperature";
        }
    }
}