using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using PeterHan.PLib.UI;

using HysteresisStorage.UI;

namespace HysteresisStorage
{
    public class HysteresisStoragePatches
    {
        internal const string ENABLE_ICON_NAME = "action_enable_hysteresis";
        internal const string DISABLE_ICON_NAME = "action_disable_hysteresis";

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

        [HarmonyPatch(typeof(FishFeederConfig))]
        [HarmonyPatch(nameof(FishFeederConfig.ConfigureBuildingTemplate))]
        public static class FishFeederConfig_ConfigureBuildingTemplate_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        [HarmonyPatch(typeof(CreatureFeederConfig))]
        [HarmonyPatch(nameof(CreatureFeederConfig.ConfigureBuildingTemplate))]
        public static class CreatureFeederConfig_ConfigureBuildingTemplate_Patch
        {
            private static void Postfix(GameObject go)
            {
                AddComponent(go);
            }
        }

        [HarmonyPatch(typeof(SolidConduitInboxConfig))]
        [HarmonyPatch(nameof(SolidConduitInboxConfig.DoPostConfigureComplete))]
        public static class SolidConduitInboxConfig_DoPostConfigureComplete_Patch
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
            delegate void CachedMethod(FilteredStorage instance);
            delegate T CachedMethod<T>(FilteredStorage instance);
            private static CachedMethod<float> GetMaxCapacityMinusStorageMarginDelegate = (CachedMethod<float>)typeof(FilteredStorage).GetMethod("GetMaxCapacityMinusStorageMargin", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(CachedMethod<float>));
            private static CachedMethod<float> GetAmountStoredDelegate = (CachedMethod<float>)typeof(FilteredStorage).GetMethod("GetAmountStored", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(CachedMethod<float>));
            private static CachedMethod<float> GetMaxCapacityDelegate = (CachedMethod<float>)typeof(FilteredStorage).GetMethod("GetMaxCapacity", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(CachedMethod<float>));
            private static CachedMethod<bool> IsFunctionalDelegate = (CachedMethod<bool>)typeof(FilteredStorage).GetMethod("IsFunctional", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(CachedMethod<bool>));
            private static CachedMethod OnFetchCompleteDelegate = (CachedMethod)typeof(FilteredStorage).GetMethod("OnFetchComplete", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(CachedMethod));

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

                float maxCapacityMinusStorageMargin = GetMaxCapacityMinusStorageMarginDelegate(__instance);
                float amountStored = GetAmountStoredDelegate(__instance);
                float num = Mathf.Max(0f, maxCapacityMinusStorageMargin - amountStored);

                bool isFunctional = IsFunctionalDelegate(__instance);

                if (num > 0f && flag && isFunctional && (amountStored <= hysteresisStorage.MinUserStorage || hysteresisStorage.onceFull == false))
                {
                    System.Action onFetchComplete = () =>
                    {
                        float _amountStored = GetAmountStoredDelegate(__instance);
                        if (_amountStored < hysteresisStorage.MinUserStorage)
                            hysteresisStorage.onceFull = false;
                        OnFetchCompleteDelegate(__instance);
                    };

                    float maxCapacity = GetMaxCapacityDelegate(__instance);
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

        internal static void LoadSprites()
        {
            Sprite enableSprite = PUIUtils.LoadSprite("HysteresisStorage.images.action_enable_hysteresis.png");
            enableSprite.name = HysteresisStoragePatches.ENABLE_ICON_NAME;
            Sprite disableSprite = PUIUtils.LoadSprite("HysteresisStorage.images.action_disable_hysteresis.png");
            disableSprite.name = HysteresisStoragePatches.DISABLE_ICON_NAME;

            Assets.Sprites.Add(HysteresisStoragePatches.ENABLE_ICON_NAME, enableSprite);
            Assets.Sprites.Add(HysteresisStoragePatches.DISABLE_ICON_NAME, disableSprite);
        }

        [HarmonyPatch(typeof(UserMenuScreen), "OnPrefabInit")]
        public static class UserMenuScreen_OnPrefabInit_Patch
        {
            internal static void Postfix(ref Sprite[] ___icons)
            {
                if (Assets.GetSprite(ENABLE_ICON_NAME) == null)
                    LoadSprites();
                ___icons = ___icons.Concat(new Sprite[] {
                    Assets.GetSprite(ENABLE_ICON_NAME),
                    Assets.GetSprite(DISABLE_ICON_NAME)
                }).ToArray();
            }
        }
    }
}
