using static STRINGS.UI;

namespace ResourceSensor
{
    public class UI
    {
        public class UISIDESCREENS
        {
            public class RESOURCE_SENSOR_SIDE_SCREEN
            {
                public static readonly LocString TITLE = "Resource Sensor";
                public static readonly LocString VALUE_NAME = "Value";
                public static LocString SLIDER_TOOLTIP = $"Resources further than {FormatAsKeyWord("{0}")} tiles will not be counted.";
            }

            public class THRESHOLD_SWITCH_SIDESCREEN
            {
                public static LocString RESOURCE_TOOLTIP_ABOVE = "Will send a " + FormatAsAutomationState("Green Signal", AutomationState.Active) + " if " + PRE_KEYWORD + "Total Mass" + PST_KEYWORD + " is above <b>{0}</b>";

                public static LocString RESOURCE_TOOLTIP_BELOW = "Will send a " + FormatAsAutomationState("Green Signal", AutomationState.Active) + " if the " + PRE_KEYWORD + "Total Mass" + PST_KEYWORD + " is below <b>{0}</b>";
            }
        }

        public class BUILDINGS
        {
            public class PREFABS
            {
                public class LOGICRESOURCESENSOR
                {
                    public static LocString NAME = FormatAsLink("Resource Sensor", LogicResourceSensorConfig.ID);

                    public static LocString DESCRIPTION = "Detecting resources can enable complex storage automations.";

                    public static LocString EFFECT = "Sends a " + FormatAsAutomationState("Green Signal", AutomationState.Active) + " or a " + FormatAsAutomationState("Red Signal", AutomationState.Standby) + " based on count mode and material amount.";

                    public static LocString LOGIC_PORT = "Material Count";

                    public static LocString LOGIC_PORT_ACTIVE = "Sends a " + FormatAsAutomationState("Green Signal", AutomationState.Active) + " if the total mass of counted resources is greater than the selected threshold.";

                    public static LocString LOGIC_PORT_INACTIVE = "Otherwise, sends a " + FormatAsAutomationState("Red Signal", AutomationState.Standby);

                    public static LocString SIDESCREEN_TITLE = "Resource Sensor";
                }
            }
        }
    }
}