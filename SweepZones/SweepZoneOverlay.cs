using System.Collections.Generic;
using UnityEngine;

namespace SweepZones
{
    public class SweepZoneOverlay : OverlayModes.Mode
    {
        public static readonly HashedString ID = new HashedString("SWEEPZONES");

        private readonly List<LegendEntry> legend = new List<LegendEntry>();

        private static readonly int cameraLayerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");

        private static readonly int selectionMask = LayerMask.GetMask(new string[] {
                "MaskedOverlay"
        });

        public SweepZoneOverlay()
        {
            for (int i = 0; i < CommonProps.PRIORITY_COLORS.Length; i++)
                legend.Add(new LegendEntry($"Priority {i + 1}", "", CommonProps.PRIORITY_COLORS[i]));

            legendFilters = CreateDefaultFilters();
        }

        internal static Color GetColor(SimDebugView _, int cell)
        {
            if (!SaveState.Instance.ContainsCell(cell))
                return Color.black;

            return CommonProps.PRIORITY_COLORS[SaveState.Instance[cell] - 1];
        }

        public override void Disable()
        {
            UnregisterSaveLoadListeners();
            CameraController.Instance.ToggleColouredOverlayView(false);
            Camera.main.cullingMask &= ~cameraLayerMask;
            SelectTool.Instance.ClearLayerMask();
            base.Disable();
        }

        public override void Enable()
        {
            base.Enable();
            RegisterSaveLoadListeners();
            CameraController.Instance.ToggleColouredOverlayView(true);
            Camera.main.cullingMask |= cameraLayerMask;
            SelectTool.Instance.SetLayerMask(selectionMask);
        }

        public override List<LegendEntry> GetCustomLegendData()
        {
            return legend;
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