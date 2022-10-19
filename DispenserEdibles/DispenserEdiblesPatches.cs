using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using TUNING;
using UnityEngine;

namespace DispenserEdibles
{
    class DispenserEdiblesPatches
    {
        readonly static List<Tag> mergedSolidFilter = STORAGEFILTERS.NOT_EDIBLE_SOLIDS.Concat(STORAGEFILTERS.FOOD).ToList();

        [HarmonyPatch(typeof(ObjectDispenserConfig))]
        [HarmonyPatch(nameof(ObjectDispenserConfig.DoPostConfigureComplete))]
        class ObjectDispenserConfig_DoPostConfigureComplete_Patch
        {
            static void Postfix(ref GameObject go)
            {
                var storage = go.GetComponent<Storage>();
                storage.storageFilters = mergedSolidFilter;
            }
        }

        [HarmonyPatch(typeof(BaseModularLaunchpadPortConfig))]
        [HarmonyPatch(nameof(BaseModularLaunchpadPortConfig.DoPostConfigureComplete))]
        class BaseModularLaunchpadPortConfig_DoPostConfigureComplete_Patch
        {
            static void Postfix(GameObject go, bool isLoader)
            {
                if (!isLoader)
                {
                    var conduitDispenser = go.GetComponent<SolidConduitDispenser>();
                    if (conduitDispenser != null && conduitDispenser.ConduitType == ConduitType.Solid)
                    {
                        var storage = conduitDispenser.GetComponent<Storage>();
                        storage.storageFilters = mergedSolidFilter;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SolidCargoBaySmallConfig))]
        [HarmonyPatch(nameof(SolidCargoBaySmallConfig.DoPostConfigureComplete))]
        class SolidCargoBaySmallConfig_DoPostConfigureComplete_Patch
        {
            static bool Prefix(ref GameObject go, float ___CAPACITY)
            {
                go = BuildingTemplates.ExtendBuildingToClusterCargoBay(go, ___CAPACITY, mergedSolidFilter, CargoBay.CargoType.Solids);
                BuildingTemplates.ExtendBuildingToRocketModuleCluster(go, null, ROCKETRY.BURDEN.MODERATE);
                return false;
            }
        }

        [HarmonyPatch(typeof(SolidCargoBayClusterConfig))]
        [HarmonyPatch(nameof(SolidCargoBayClusterConfig.DoPostConfigureComplete))]
        class SolidCargoBayClusterConfig_DoPostConfigureComplete_Patch
        {
            static bool Prefix(ref GameObject go, float ___CAPACITY)
            {
                go = BuildingTemplates.ExtendBuildingToClusterCargoBay(go, ___CAPACITY, mergedSolidFilter, CargoBay.CargoType.Solids);
                BuildingTemplates.ExtendBuildingToRocketModuleCluster(go, null, ROCKETRY.BURDEN.MAJOR);
                return false;
            }
        }
    }
}
