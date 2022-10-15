using HarmonyLib;
using KMod;

namespace HysteresisStorage
{
    public class HysteresisStorageMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            LocString.CreateLocStringKeys(typeof(HysteresisStorage.UI.STRINGS));
        }
    }
}