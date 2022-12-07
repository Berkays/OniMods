using System;
using System.Reflection;
using PeterHan.PLib.Core;

namespace SweepZones
{
    internal static class ModIntegrations
    {
        internal static void LoadIntegrations()
        {
            Type forbidItemsType = PPatchTools.GetTypeSafe(ForbidItemsConfiguration.NAMESPACE);

            if (forbidItemsType != null)
            {
                ForbidItemsConfiguration.Enabled = true;
                ForbidItemsConfiguration.Tag = (Tag)forbidItemsType.GetField("Forbidden", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            }
        }

        internal static class ForbidItemsConfiguration
        {
            internal const string NAMESPACE = "PeterHan.ForbidItems.ForbidItemsPatches";

            internal static bool Enabled = false;
            internal static Tag Tag { get; set; }
        }
    }
}