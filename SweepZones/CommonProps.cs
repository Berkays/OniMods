using UnityEngine;

namespace SweepZones
{
    internal static class CommonProps
    {
        internal static readonly Color32 TOOL_COLOR = new Color32(255, 172, 52, 255);
        internal static readonly Color32 FORBID_COLOR = new Color32(60, 230, 60, 150);

        internal static readonly Color[] PRIORITY_COLORS = new Color[] {
        new Color32(102, 240, 248, 180),
        new Color32(42, 194, 243, 180),
        new Color32(32, 126, 247, 180),
        new Color32(207, 118, 255, 180),
        new Color32(190, 50, 255, 180),
        new Color32(135, 0, 210, 180),
        new Color32(254, 105, 109, 180),
        new Color32(255, 60, 51, 180),
        new Color32(252, 3, 0, 180),
        FORBID_COLOR
        };
    }
}