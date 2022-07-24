using System.Linq;

using HarmonyLib;
using TUNING;
using UnityEngine;

namespace DispenserEdibles
{
    public class DispenserEdiblesPatches
    {
        [HarmonyPatch(typeof(ObjectDispenserConfig))]
        [HarmonyPatch(nameof(ObjectDispenserConfig.DoPostConfigureComplete))]
        public class ObjectDispenserConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(ref GameObject go)
            {
                var storage = go.GetComponent<Storage>();
                storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS.Concat(STORAGEFILTERS.FOOD).ToList();
            }
        }
    }
}
