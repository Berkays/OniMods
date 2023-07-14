using System;
using PeterHan.PLib.Core;

namespace HysteresisStorage
{
    internal static class ModIntegrations
    {
        internal static void LoadIntegrations()
        {
            Type t = PPatchTools.GetTypeSafe(StoragePodConfiguration.StoragePodBuildingConfig, StoragePodConfiguration.NAMESPACE);

            if (t != null)
            {
                StoragePodConfiguration.Enabled = true;
            }
        }

        internal static class StoragePodConfiguration
        {
            internal const string NAMESPACE = "FixStoragePod";

            internal static bool Enabled = false;
            internal static string StoragePodBuildingConfig = "FixPack.StoragePod.StoragePodConfig";
            internal static string StoragePodBuildingConfigMethod = "DoPostConfigureComplete";
            internal static string CoolPodBuildingConfig = "FixPack.StoragePod.CoolPodConfig";
            internal static string CoolPodBuildingConfigMethod = "DoPostConfigureComplete";
        }
    }
}