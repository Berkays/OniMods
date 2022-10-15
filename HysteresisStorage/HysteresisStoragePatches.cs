using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using HysteresisStorage.UI;

namespace HysteresisStorage
{
    public class HysteresisStoragePatches
    {
        [HarmonyPatch(typeof(DetailsScreen))]
        [HarmonyPatch("OnPrefabInit")]
        public static class DetailsScreen_OnPrefabInit_Patch
        {
            public static void Postfix()
            {
                FUtility.FUI.SideScreen.AddClonedSideScreen<HysteresisStorageSideScreen>(
                    "HysteresisStorageSideScreen",
                    "Capacity Control Side Screen",
                    typeof(CapacityControlSideScreen));
            }
        }

        private static void AddComponent(GameObject go)
        {
            go.AddOrGet<ToggleButton>();
            go.AddOrGet<HysteresisStorageLogic>();
        }

        [HarmonyPatch(typeof(RationBoxConfig))]
        [HarmonyPatch(nameof(RationBoxConfig.ConfigureBuildingTemplate))]
        public static class RationBoxConfig_ConfigureBuildingTemplate_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        [HarmonyPatch(typeof(StorageLockerConfig))]
        [HarmonyPatch(nameof(StorageLockerConfig.ConfigureBuildingTemplate))]
        public static class StorageLockerConfig_ConfigureBuildingTemplate_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        [HarmonyPatch(typeof(StorageLockerSmartConfig))]
        [HarmonyPatch(nameof(StorageLockerSmartConfig.DoPostConfigureComplete))]
        public static class StorageLockerSmartConfig_DoPostConfigureComplete_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        [HarmonyPatch(typeof(RefrigeratorConfig))]
        [HarmonyPatch(nameof(RefrigeratorConfig.DoPostConfigureComplete))]
        public static class RefrigeratorConfig_DoPostConfigureComplete_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        // [HarmonyPatch(typeof(FishFeederConfig))]
        // [HarmonyPatch(nameof(FishFeederConfig.ConfigureBuildingTemplate))]
        // public static class FishFeederConfig_ConfigureBuildingTemplate_Patch
        // {
        //     private static void Postfix(GameObject go)
        //     {
        //         AddComponent(go);
        //     }
        // }

        [HarmonyPatch(typeof(CreatureFeederConfig))]
        [HarmonyPatch(nameof(CreatureFeederConfig.ConfigureBuildingTemplate))]
        public static class CreatureFeederConfig_ConfigureBuildingTemplate_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        [HarmonyPatch(typeof(FilteredStorage))]
        [HarmonyPatch("OnFilterChanged")]
        public class FilteredStorage_OnFilterChanged_Patch
        {
            public static bool Prefix(HashSet<Tag> tags, FilteredStorage __instance, ref FetchList2 ___fetchList, Storage ___storage, ChoreType ___choreType, Tag[] ___forbiddenTags)
            {
                bool flag = tags != null && tags.Count != 0;
                if (___fetchList != null)
                {
                    ___fetchList.Cancel("");
                    ___fetchList = null;
                }

                HysteresisStorageLogic hysteresisStorage = ___storage.GetComponent<HysteresisStorageLogic>();
                if (hysteresisStorage == null)
                    return true;

                hysteresisStorage.ForceCapacityChangeRefresh();

                // Operate normally
                if (hysteresisStorage.HysteresisEnabled == false)
                    return true;

                var GetMaxCapacityMinusStorageMarginMethodInfo = typeof(FilteredStorage).GetMethod("GetMaxCapacityMinusStorageMargin", BindingFlags.Instance | BindingFlags.NonPublic);
                var GetAmountStoredMethodInfo = typeof(FilteredStorage).GetMethod("GetAmountStored", BindingFlags.Instance | BindingFlags.NonPublic);
                var GetMaxCapacityMethodInfo = typeof(FilteredStorage).GetMethod("GetMaxCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
                var IsFunctionalMethodInfo = typeof(FilteredStorage).GetMethod("IsFunctional", BindingFlags.Instance | BindingFlags.NonPublic);
                var OnFetchCompleteMethodInfo = typeof(FilteredStorage).GetMethod("OnFetchComplete", BindingFlags.Instance | BindingFlags.NonPublic);

                float maxCapacityMinusStorageMargin = (float)GetMaxCapacityMinusStorageMarginMethodInfo.Invoke(__instance, new object[0]);
                float amountStored = (float)GetAmountStoredMethodInfo.Invoke(__instance, new object[0]);
                float num = Mathf.Max(0f, maxCapacityMinusStorageMargin - amountStored);

                bool isFunctional = (bool)IsFunctionalMethodInfo.Invoke(__instance, new object[0]);

                if (num > 0f && flag && isFunctional && (amountStored <= hysteresisStorage.MinUserStorage || hysteresisStorage.onceFull == false))
                {
                    System.Action onFetchComplete = () =>
                    {
                        float _amountStored = (float)GetAmountStoredMethodInfo.Invoke(__instance, new object[0]);
                        if (_amountStored < hysteresisStorage.MinUserStorage)
                            hysteresisStorage.onceFull = false;
                        OnFetchCompleteMethodInfo.Invoke(__instance, new object[0]);
                    };

                    float maxCapacity = (float)GetMaxCapacityMethodInfo.Invoke(__instance, new object[0]);
                    num = Mathf.Max(0f, maxCapacity - amountStored);

                    ___fetchList = new FetchList2(___storage, ___choreType);
                    ___fetchList.ShowStatusItem = false;
                    ___fetchList.Add(tags, ___forbiddenTags, num, Operational.State.Functional);
                    ___fetchList.Submit(onFetchComplete, check_storage_contents: false);

                    return false;
                }

                if (flag && isFunctional)
                {
                    if (amountStored <= hysteresisStorage.MinUserStorage)
                        hysteresisStorage.onceFull = false;
                    else if (__instance.IsFull())
                        hysteresisStorage.onceFull = true;
                }

                return false;
            }
        }
    }
}
