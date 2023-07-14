using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

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

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);

            ModIntegrations.LoadIntegrations();

            if (ModIntegrations.StoragePodConfiguration.Enabled)
            {
                var patchMethod = new HarmonyMethod(typeof(HysteresisStoragePatches.StoragePod_DoPostConfigureComplete_Patch).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public));
                harmony.Patch(
                    PPatchTools.GetTypeSafe(ModIntegrations.StoragePodConfiguration.StoragePodBuildingConfig, ModIntegrations.StoragePodConfiguration.NAMESPACE).GetMethod(ModIntegrations.StoragePodConfiguration.StoragePodBuildingConfigMethod, BindingFlags.Public | BindingFlags.Instance),
                 postfix: patchMethod);
                harmony.Patch(
                    PPatchTools.GetTypeSafe(ModIntegrations.StoragePodConfiguration.CoolPodBuildingConfig).GetMethod(ModIntegrations.StoragePodConfiguration.CoolPodBuildingConfigMethod, BindingFlags.Public | BindingFlags.Instance),
                 postfix: patchMethod);
            }
        }

    }
}