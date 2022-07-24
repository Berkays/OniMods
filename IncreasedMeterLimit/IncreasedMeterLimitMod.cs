using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace IncreasedMeterLimit
{
    class IncreasedMeterLimitMod : KMod.UserMod2
    {
        internal static ModOptions Config;
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);

            new POptions().RegisterOptions(this, typeof(IncreasedMeterLimit.ModOptions));
            Config = POptions.ReadSettings<ModOptions>();
            if (Config == null)
                Config = new ModOptions();
        }
    }
}
