using UnityEngine;
using PeterHan.PLib.UI;

namespace AutomationBypass
{
    public static class ICONS
    {
        private static Sprite bypassedPort;
        public static Sprite BYPASSED_PORT_SPRITE
        {
            get
            {
                if (bypassedPort == null)
                    loadIcons();
                return bypassedPort;
            }
            set
            {
                bypassedPort = value;
            }
        }

        private static void loadIcons()
        {
            BYPASSED_PORT_SPRITE = PUIUtils.LoadSprite("AutomationBypass.images.bypassed_port.png");
            BYPASSED_PORT_SPRITE.name = AutomationBypass.UI.STRINGS.BYPASSED_PORT;
        }
    }
}