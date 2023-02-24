using System.Collections.Generic;

namespace SweepZones
{
    public class UI
    {
        public class STRINGS
        {
            // TOOL
            public const string TOOL_TITLE = "Set Sweep Zones";
            public const string TOOL_HOVER_CARD_TITLE = "Sweep Zone Tool";
            public const string TOOL_DESCRIPTION = "Set Sweep Zones {Hotkey}";
            public const string TOOL_ICON = "SWEEPZONES.TOOL.ICON";
            public const string SET_VISUALIZER_ICON = "SWEEPZONES.VISUALIZER.SET.ICON";
            public const string CANCEL_VISUALIZER_ICON = "SWEEPZONES.VISUALIZER.CANCEL.ICON";

            // OVERLAY
            public static readonly KeyValuePair<string, string> OVERLAY_NAME = new KeyValuePair<string, string>("OVERLAY.NAME", "Sweep Zone Overlay");
            public static readonly KeyValuePair<string, string> OVERLAY_DESCRIPTION = new KeyValuePair<string, string>("OVERLAY.DESCRIPTION", "Display auto sweep zone areas");
            public const string OVERLAY_ICON = "SWEEPZONES.TOOL.ICON";

            public static readonly KeyValuePair<string, string> OVERLAY_STATE_SWEEP = new KeyValuePair<string, string>("SweepZones.Overlay.STATE_SWEEP", "Sweep Zones");
            public static readonly KeyValuePair<string, string> OVERLAY_STATE_MOP = new KeyValuePair<string, string>("SweepZones.Overlay.STATE_MOP", "Mop Zones");
        }
    }

    public class Actions
    {
        public static string OVERLAY_ACTION_KEY = "SweepZones.Action.Overlay";
        public static LocString OVERLAY_ACTION_TITLE = "Sweep Zones Overlay";
        public static string TOOL_ACTION_KEY = "SweepZones.Action.Tool";
        public static LocString TOOL_ACTION_TITLE = "Sweep Zones Tool";
    }
}