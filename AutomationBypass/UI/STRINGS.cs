namespace AutomationBypass
{
    public class UI
    {
        public class OVERLAYS
        {
            public class LOGIC
            {
                public class TOOLTIPS
                {
                    public static LocString BYPASSED = "Bypassed input ports ignore automation wires";
                }

                public static LocString BYPASSED = "Bypassed";
            }
        }

        public class STRINGS
        {
            public const string BYPASSED_PORT = "AUTOMATIONBYPASS.BYPASSED_PORT.ICON";
        }

        public class USERMENUACTIONS
        {
            public class AutomationBypass
            {
                public static LocString NAME = "Ignore Automation";
                public static LocString NAME_OFF = "Receive Automation";
            }
        }

        public static class MISC
        {
            public static class STATUSITEMS
            {
                public static class BYPASSED
                {
                    public static LocString NAME = "Automation Bypassed";
                    public static LocString TOOLTIP = "Automation input is ignored";
                }
            }
        }
    }
}