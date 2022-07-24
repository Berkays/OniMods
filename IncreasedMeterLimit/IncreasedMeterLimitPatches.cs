using HarmonyLib;
using UnityEngine;

namespace IncreasedMeterLimit
{
    public class IncreasedMeterLimitPatches
    {
        static NonLinearSlider.Range[] GetSlider(float max) => max == 5000f ?  GetDefaultSlider() : GetNonDefaultSlider(max); 
        static NonLinearSlider.Range[] GetDefaultSlider() => new NonLinearSlider.Range[3]
        {
            new NonLinearSlider.Range(30f, 200f),
            new NonLinearSlider.Range(60f, 3500f),
            new NonLinearSlider.Range(10f, 5000f)
        };

        static NonLinearSlider.Range[] GetNonDefaultSlider(float max) => new NonLinearSlider.Range[2]
        {
            new NonLinearSlider.Range(30f, 200f),
            new NonLinearSlider.Range(70f, max)
        };

        [HarmonyPatch(typeof(LiquidLimitValveConfig))]
        [HarmonyPatch(nameof(LiquidLimitValveConfig.ConfigureBuildingTemplate))]
        public class LiquidLimitValveConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(ref GameObject go)
            {
                var limitValve = go.GetComponent<LimitValve>();
                limitValve.maxLimitKg = IncreasedMeterLimitMod.Config.LiquidMeterLimit;
                limitValve.sliderRanges = GetSlider(limitValve.maxLimitKg);
            }
        }

        [HarmonyPatch(typeof(GasLimitValveConfig))]
        [HarmonyPatch(nameof(GasLimitValveConfig.ConfigureBuildingTemplate))]
        public class GasLimitValveConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(ref GameObject go)
            {
                var limitValve = go.GetComponent<LimitValve>();
                limitValve.maxLimitKg = IncreasedMeterLimitMod.Config.GasMeterLimit;
                limitValve.sliderRanges = GetSlider(limitValve.maxLimitKg);
            }
        }

        [HarmonyPatch(typeof(SolidLimitValveConfig))]
        [HarmonyPatch(nameof(SolidLimitValveConfig.DoPostConfigureComplete))]
        public class SolidLimitValveConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(ref GameObject go)
            {
                var limitValve = go.GetComponent<LimitValve>();
                limitValve.maxLimitKg = IncreasedMeterLimitMod.Config.ConveyorMeterLimit;
                limitValve.sliderRanges = GetSlider(limitValve.maxLimitKg);
            }
        }
    }
}
