using Klei.AI;
using HarmonyLib;
using UnityEngine;

namespace NoRadiationExposure
{
    public class NoRadiationExposurePatches
    {
        [HarmonyPatch(typeof(RadiationMonitor))]
        [HarmonyPatch("CheckRadiationLevel")]
        public class RadiationMonitor_CheckRadiationLevel_Patch
        {
            public static void _CheckRadiationLevel(RadiationMonitor.Instance smi, float dt)
            {
                Traverse.Create<RadiationMonitor>().Method("RadiationRecovery", new object[] { smi, dt }).GetValue(smi, dt);

                smi.sm.timeUntilNextExposureReact.Delta(0f - dt, smi);
                smi.sm.timeUntilNextSickReact.Delta(0f - dt, smi);
                int num = Grid.PosToCell(smi.gameObject);
                if (Grid.IsValidCell(num))
                {
                    float num2 = Mathf.Clamp01(1f - Db.Get().Attributes.RadiationResistance.Lookup(smi.gameObject).GetTotalValue());
                    float num3 = Grid.Radiation[num] * 1f * num2 / 600f * dt;
                    smi.master.gameObject.GetAmounts().Get(Db.Get().Amounts.RadiationBalance).ApplyDelta(num3);
                    float num4 = num3 / dt * 600f;
                    smi.sm.currentExposurePerCycle.Set(num4, smi);
                    // if (smi.sm.timeUntilNextExposureReact.Get(smi) <= 0f && !smi.HasTag(GameTags.InTransitTube) && COMPARE_REACT(smi, num4))
                    // {
                    //     smi.sm.timeUntilNextExposureReact.Set(120f, smi);
                    //     Emote radiation_Glare = Db.Get().Emotes.Minion.Radiation_Glare;
                    //     smi.master.gameObject.GetSMI<ReactionMonitor.Instance>().AddSelfEmoteReactable(smi.master.gameObject, "RadiationReact", radiation_Glare, isOneShot: true, Db.Get().ChoreTypes.EmoteHighPriority);
                    // }
                }
                if (smi.sm.timeUntilNextSickReact.Get(smi) <= 0f && smi.sm.isSick.Get(smi) && !smi.HasTag(GameTags.InTransitTube))
                {
                    smi.sm.timeUntilNextSickReact.Set(60f, smi);
                    Emote radiation_Itch = Db.Get().Emotes.Minion.Radiation_Itch;
                    smi.master.gameObject.GetSMI<ReactionMonitor.Instance>().AddSelfEmoteReactable(smi.master.gameObject, "RadiationReact", radiation_Itch, isOneShot: true, Db.Get().ChoreTypes.RadiationPain);
                }
                smi.sm.radiationExposure.Set(smi.master.gameObject.GetComponent<KSelectable>().GetAmounts().GetValue("RadiationBalance"), smi);
            }

            public static bool Prefix(RadiationMonitor.Instance smi, float dt)
            {
                _CheckRadiationLevel(smi, dt);
                return false;
            }
        }
    }
}
