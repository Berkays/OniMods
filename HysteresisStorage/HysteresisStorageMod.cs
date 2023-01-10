using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace HysteresisStorage
{
    public class HysteresisStorageMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            LocString.CreateLocStringKeys(typeof(HysteresisStorage.UI.STRINGS));
        }
    }
}