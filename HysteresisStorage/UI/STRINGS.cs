using static STRINGS.UI;

namespace HysteresisStorage.UI
{
    public static class STRINGS
    {
        public static class HYSTERESISSTORAGESIDESCREEN
        {
            public static LocString SLIDER_TITLE = "Min Value";
            public static LocString SLIDER_TOOLTIP = $"Storage bin will not request materials until {FormatAsKeyWord("{0}{1}")}.";
            public static LocString SLIDER_MIN_LABEL = $"Min:";
            public static LocString CHECKBOX_TITLE = $"Hysteresis";
        }

        public static class USERMENUACTIONS
        {
            public static LocString NAME = "Disable Hysteresis";
            public static LocString TOOLTIP = "Disable storage hysteresis";
            public static LocString NAME_OFF = "Enable Hysteresis";
            public static LocString TOOLTIP_OFF = "Enable storage hysteresis";
        }
    }
}