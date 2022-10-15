using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TUNING;
using UnityEngine;

namespace HysteresisStorage.BACKUP
{
    public class HysteresisStoragePatchesBACKUP
    {
        // [HarmonyPatch(typeof(BuildingLoader), "CreateBuildingComplete")]
        // public class Patch_BuildingConfigManager_OnPrefabInit
        // {
        //     internal static void Postfix(ref GameObject __result)
        //     {
        //         var filteredStorage = __result.GetComponent<FilteredStorage>();
        //         if (filteredStorage != null)
        //         {
        //             __result.AddOrGet<HysteresisStorageSlider>();
        //             return;
        //         }

        //         var storageLocker = __result.GetComponent<StorageLocker>();
        //         if (storageLocker != null)
        //         {
        //             var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        //             var field = storageLocker.GetType().GetField("filteredStorage", bindingFlags);
        //             filteredStorage = (FilteredStorage)field?.GetValue(storageLocker);

        //             if (filteredStorage != null)
        //                 __result.AddOrGet<HysteresisStorageSlider>();
        //         }


        //     }
        // }

        // [HarmonyPatch(typeof(RationBoxConfig))]
        // [HarmonyPatch("ConfigureBuildingTemplate")]
        // public static class Patch_RationBoxConfig_ConfigureBuildingTemplate
        // {
        //     internal static bool Prefix(GameObject go, RationBox __instance)
        //     {
        //         Prioritizable.AddRef(go);
        //         Storage storage = go.AddOrGet<Storage>();
        //         storage.capacityKg = 150f;
        //         storage.showInUI = true;
        //         storage.showDescriptor = true;
        //         storage.storageFilters = STORAGEFILTERS.FOOD;
        //         storage.allowItemRemoval = true;
        //         storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
        //         storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
        //         storage.showCapacityStatusItem = true;
        //         storage.showCapacityAsMainStatus = true;
        //         go.AddOrGet<TreeFilterable>();
        //         go.AddOrGet<RationBox>();
        //         go.AddOrGet<HysteresisStorageSlider>();
        //         go.AddOrGet<UserNameable>();
        //         go.AddOrGetDef<RocketUsageRestriction.Def>();

        //         return false;
        //     }
        // }

        // [HarmonyPatch(typeof(RationBox))]
        // [HarmonyPatch("OnPrefabInit")]
        // public static class Patch_RationBox_ConfigureBuildingTemplate
        // {
        //     internal static void Postfix(RationBox __instance)
        //     {
        //         var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        //         var field = __instance.GetType().GetField("filteredStorage", bindingFlags);
        //         var slider = __instance.gameObject.AddOrGet<HysteresisStorageSlider>();
        //         slider.storage = (FilteredStorage)field?.GetValue(__instance);
        //     }
        // }

        // private static void AddComponent(GameObject go)
        // {
        //     var toggleButton = go.AddOrGet<ToggleButton>();
        //     var slider = go.AddOrGet<HysteresisStorageSlider>();
        //     toggleButton.slider = slider;
        // }

        // [HarmonyPatch(typeof(RationBoxConfig))]
        // [HarmonyPatch("ConfigureBuildingTemplate")]
        // public static class RationBoxConfig_ConfigureBuildingTemplate_Patch
        // {
        //     private static void Postfix(GameObject go)
        //     {
        //         AddComponent(go);
        //     }
        // }

        // [HarmonyPatch(typeof(StorageLockerConfig))]
        // [HarmonyPatch("ConfigureBuildingTemplate")]
        // public static class StorageLockerConfig_ConfigureBuildingTemplate_Patch
        // {
        //     private static void Postfix(GameObject go)
        //     {
        //         AddComponent(go);
        //     }
        // }

        // // [HarmonyPatch(typeof(StorageLockerSmartConfig))]
        // // [HarmonyPatch("DoPostConfigureComplete")]
        // // public static class StorageLockerSmartConfig_DoPostConfigureComplete_Patch
        // // {
        // //     private static void Postfix(GameObject go)
        // //     {
        // //         AddComponent(go);
        // //     }
        // // }

        // [HarmonyPatch(typeof(RefrigeratorConfig))]
        // [HarmonyPatch("DoPostConfigureComplete")]
        // public static class RefrigeratorConfig_DoPostConfigureComplete_Patch
        // {
        //     private static void Postfix(GameObject go)
        //     {
        //         AddComponent(go);
        //     }
        // }

        // [HarmonyPatch(typeof(TreeFilterableSideScreen))]
        // [HarmonyPatch("AddRow")]
        // public static class TreeFilterableSideScreen_AddRow_Patch
        // {
        //     private static bool Prefix(Tag rowTag, TreeFilterableSideScreen __instance, UIPool<TreeFilterableSideScreenRow> ___rowPool, GameObject ___rowGroup, Dictionary<Tag, TreeFilterableSideScreenRow> ___tagRowMap, TreeFilterable ___targetFilterable)
        //     {
        //         if (___tagRowMap.ContainsKey(rowTag) == true)
        //             return false;


        //         TreeFilterableSideScreenRow freeElement = ___rowPool.GetFreeElement(___rowGroup, forceActive: true);
        //         freeElement.Parent = __instance;


        //         ___tagRowMap.Add(rowTag, freeElement);

        //         var GetTagsSortedAlphabeticallyMethodInfo = typeof(TreeFilterableSideScreen).GetMethod("GetTagsSortedAlphabetically", BindingFlags.Instance | BindingFlags.NonPublic);
        //         object sortedTags = GetTagsSortedAlphabeticallyMethodInfo.Invoke(__instance, new object[] { DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(rowTag) });

        //         var tagOrderInfoType = __instance.GetType().GetNestedType("TagOrderInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        //         var field = tagOrderInfoType.GetField("tag", BindingFlags.Public | BindingFlags.Instance);

        //         Dictionary<Tag, bool> dictionary = new Dictionary<Tag, bool>();
        //         foreach (var item in sortedTags as IEnumerable)
        //         {
        //             Tag _tag = (Tag)field?.GetValue(item);

        //             if (dictionary.ContainsKey(_tag) == true)
        //                 continue;
        //             dictionary.Add(_tag, ___targetFilterable.ContainsTag(_tag) || ___targetFilterable.ContainsTag(rowTag));
        //         }

        //         freeElement.SetElement(rowTag, ___targetFilterable.ContainsTag(rowTag), dictionary);
        //         freeElement.transform.SetAsLastSibling();

        //         return false;
        //     }
        // }

        // [HarmonyPatch(typeof(FilteredStorage))]
        // [HarmonyPatch("OnFilterChanged")]
        // public class FilteredStorage_OnFilterChanged_Patch
        // {
        //     public static bool Prefix(HashSet<Tag> tags, FilteredStorage __instance, ref FetchList2 ___fetchList, Storage ___storage, ChoreType ___choreType, Tag[] ___forbiddenTags)
        //     {
        //         bool flag = tags != null && tags.Count != 0;
        //         if (___fetchList != null)
        //         {
        //             ___fetchList.Cancel("");
        //             ___fetchList = null;
        //         }

        //         HysteresisStorageSlider minSlider = ___storage.GetComponent<HysteresisStorageSlider>();
        //         if (minSlider == null)
        //             return true;

        //         minSlider.OnCapacityChanged();

        //         // Operate normally
        //         if (minSlider.HysteresisEnabled == false)
        //             return true;

        //         var GetMaxCapacityMinusStorageMarginMethodInfo = typeof(FilteredStorage).GetMethod("GetMaxCapacityMinusStorageMargin", BindingFlags.Instance | BindingFlags.NonPublic);
        //         var GetAmountStoredMethodInfo = typeof(FilteredStorage).GetMethod("GetAmountStored", BindingFlags.Instance | BindingFlags.NonPublic);
        //         var GetMaxCapacityMethodInfo = typeof(FilteredStorage).GetMethod("GetMaxCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
        //         var IsFunctionalMethodInfo = typeof(FilteredStorage).GetMethod("IsFunctional", BindingFlags.Instance | BindingFlags.NonPublic);
        //         var OnFetchCompleteMethodInfo = typeof(FilteredStorage).GetMethod("OnFetchComplete", BindingFlags.Instance | BindingFlags.NonPublic);

        //         float maxCapacityMinusStorageMargin = (float)GetMaxCapacityMinusStorageMarginMethodInfo.Invoke(__instance, new object[0]);
        //         float amountStored = (float)GetAmountStoredMethodInfo.Invoke(__instance, new object[0]);
        //         float num = Mathf.Max(0f, maxCapacityMinusStorageMargin - amountStored);

        //         bool isFunctional = (bool)IsFunctionalMethodInfo.Invoke(__instance, new object[0]);

        //         if (num > 0f && flag && isFunctional && (amountStored <= minSlider.MinUserStorage || minSlider.onceFull == false))
        //         {
        //             System.Action onFetchComplete = () =>
        //             {
        //                 if ((float)GetAmountStoredMethodInfo.Invoke(__instance, new object[0]) < minSlider.MinUserStorage)
        //                     minSlider.onceFull = false;
        //                 OnFetchCompleteMethodInfo.Invoke(__instance, new object[0]);
        //             };

        //             float maxCapacity = (float)GetMaxCapacityMethodInfo.Invoke(__instance, new object[0]);
        //             num = Mathf.Max(0f, maxCapacity - amountStored);

        //             ___fetchList = new FetchList2(___storage, ___choreType);
        //             ___fetchList.ShowStatusItem = false;
        //             ___fetchList.Add(tags, ___forbiddenTags, num, Operational.State.Functional);
        //             ___fetchList.Submit(onFetchComplete, check_storage_contents: false);

        //             return false;
        //         }

        //         if (flag && isFunctional)
        //         {
        //             if (amountStored <= minSlider.MinUserStorage)
        //                 minSlider.onceFull = false;
        //             else if (__instance.IsFull())
        //                 minSlider.onceFull = true;
        //         }

        //         return false;
        //     }
        // }
    }
}
