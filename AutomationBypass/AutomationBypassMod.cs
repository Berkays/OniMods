using HarmonyLib;
using PeterHan.PLib.Core;

namespace AutomationBypass
{
    public class AutomationBypassMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            LocString.CreateLocStringKeys(typeof(UI.MISC));
        }
    }
}