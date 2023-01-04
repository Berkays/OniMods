using HarmonyLib;

/// Mod art By 3GuB
namespace ResourceSensor
{
    public class ResourceSensorPatches
    {
        internal static CapacityControlSideScreen tempSliderSideScreen;
        [HarmonyPatch(typeof(DetailsScreen))]
        [HarmonyPatch("OnPrefabInit")]
        public static class DetailsScreen_OnPrefabInit_Patch
        {
            public static void Postfix()
            {
                tempSliderSideScreen = FUtility.FUI.SideScreen.AddClonedSideScreen<CapacityControlSideScreen>(
                    "TempSliderSideScreen",
                    "Capacity Control Side Screen",
                    typeof(CapacityControlSideScreen), false);

                FUtility.FUI.SideScreen.AddClonedSideScreen<ResourceSensorSideScreen>(
                    "ResourceSensorSideScreen",
                    "LogicCritterCountSensor SideScreen",
                    typeof(CritterSensorSideScreen));
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{LogicResourceSensorConfig.ID.ToUpperInvariant()}.NAME", UI.BUILDINGS.PREFABS.LOGICRESOURCESENSOR.NAME);
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{LogicResourceSensorConfig.ID.ToUpperInvariant()}.DESC", UI.BUILDINGS.PREFABS.LOGICRESOURCESENSOR.DESCRIPTION);
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{LogicResourceSensorConfig.ID.ToUpperInvariant()}.EFFECT", UI.BUILDINGS.PREFABS.LOGICRESOURCESENSOR.EFFECT);
                ModUtil.AddBuildingToPlanScreen("Automation", LogicResourceSensorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Database.Techs))]
        [HarmonyPatch("Init")]
        public static class Database_Techs_Init_Patch
        {
            public static void Postfix(Database.Techs __instance)
            {
                __instance.TryGet("GenericSensors").unlockedItemIDs.Add(LogicResourceSensorConfig.ID);
            }
        }
    }
}