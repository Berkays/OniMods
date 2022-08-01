using Klei.AI;
using System;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

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

                double num2 = (double)smi.sm.timeUntilNextSickReact.Delta(-dt, smi);
                int cell = Grid.PosToCell(smi.gameObject);
                if (Grid.IsValidCell(cell))
                {
                    float num3 = Mathf.Clamp01(1f - Db.Get().Attributes.RadiationResistance.Lookup(smi.gameObject).GetTotalValue());
                    float delta = (float)((double)Grid.Radiation[cell] * 1.0 * (double)num3 / 600.0) * dt;
                    double num4 = (double)smi.master.gameObject.GetAmounts().Get(Db.Get().Amounts.RadiationBalance).ApplyDelta(delta);
                    float p = (float)((double)delta / (double)dt * 600.0);
                    double num5 = (double)smi.sm.currentExposurePerCycle.Set(p, smi);
                    // if ((double)smi.sm.timeUntilNextExposureReact.Get(smi) <= 0.0 && RadiationMonitor.COMPARE_REACT(smi, p))
                    // {
                    //     double num6 = (double)smi.sm.timeUntilNextExposureReact.Set(120f, smi);
                    //     ReactionMonitor.Instance smi1 = smi.master.gameObject.GetSMI<ReactionMonitor.Instance>();
                    //     SelfEmoteReactable selfEmoteReactable = new SelfEmoteReactable(smi.master.gameObject, (HashedString)"RadiationReact", Db.Get().ChoreTypes.EmoteHighPriority, (HashedString)"anim_react_radiation_kanim");
                    //     selfEmoteReactable.AddStep(new EmoteReactable.EmoteStep()
                    //     {
                    //         anim = (HashedString)"react_radiation_glare"
                    //     });
                    //     SelfEmoteReactable reactable = selfEmoteReactable;
                    //     smi1.AddOneshotReactable(reactable);
                    // }
                }
                if ((double)smi.sm.timeUntilNextSickReact.Get(smi) <= 0.0 && smi.sm.isSick.Get(smi))
                {
                    double num7 = (double)smi.sm.timeUntilNextSickReact.Set(60f, smi);
                    ReactionMonitor.Instance smi2 = smi.master.gameObject.GetSMI<ReactionMonitor.Instance>();
                    SelfEmoteReactable selfEmoteReactable = new SelfEmoteReactable(smi.master.gameObject, (HashedString)"RadiationReact", Db.Get().ChoreTypes.RadiationPain, (HashedString)"anim_react_radiation_kanim");
                    selfEmoteReactable.AddStep(new EmoteReactable.EmoteStep()
                    {
                        anim = (HashedString)"react_radiation_itch"
                    });
                    SelfEmoteReactable reactable = selfEmoteReactable;
                    smi2.AddOneshotReactable(reactable);
                }
                double num8 = (double)smi.sm.radiationExposure.Set(smi.master.gameObject.GetComponent<KSelectable>().GetAmounts().GetValue("RadiationBalance"), smi);
            }


            public static bool Prefix(RadiationMonitor.Instance smi, float dt)
            {
                _CheckRadiationLevel(smi, dt);
                return false;
            }
        }

        [HarmonyPatch(typeof(RadiationMonitor))]
        [HarmonyPatch("InitializeStates")]
        public class RadiationMonitor_InitializeStates_Patch
        {
            public static bool Prefix(RadiationMonitor __instance, out StateMachine.BaseState default_state)
            {
                PropertyInfo prop = typeof(RadiationMonitor).GetProperty("serializable");
                prop.SetValue(__instance, StateMachine.SerializeType.ParamsOnly, null);

                var createVomitChore = Traverse.Create<RadiationMonitor>().Method("CreateVomitChore", new Type[] { typeof(RadiationMonitor.Instance) });

                default_state = (StateMachine.BaseState)__instance.init;
                __instance.init.Transition((GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)null, (StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Transition.ConditionCallback)(smi => !Sim.IsRadiationEnabled())).Transition((GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active, (StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Transition.ConditionCallback)(smi => Sim.IsRadiationEnabled()));
                __instance.active.Update(new System.Action<RadiationMonitor.Instance, float>(RadiationMonitor_CheckRadiationLevel_Patch._CheckRadiationLevel), UpdateRate.SIM_4000ms, false).DefaultState(__instance.active.idle);
                __instance.active.idle.DoNothing().ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, __instance.active.sick.deadly, RadiationMonitor.COMPARE_GTE_DEADLY).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, (GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active.sick.extreme, RadiationMonitor.COMPARE_GTE_EXTREME).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, (GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active.sick.major, RadiationMonitor.COMPARE_GTE_MAJOR).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, (GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active.sick.minor, RadiationMonitor.COMPARE_GTE_MINOR);
                __instance.active.sick.ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, __instance.active.idle, RadiationMonitor.COMPARE_LT_MINOR).Enter((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State.Callback)(smi => smi.sm.isSick.Set(true, smi))).Exit((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State.Callback)(smi => smi.sm.isSick.Set(false, smi)));
                __instance.active.sick.minor.ToggleEffect(RadiationMonitor.minorSicknessEffect).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, __instance.active.sick.deadly, RadiationMonitor.COMPARE_GTE_DEADLY).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, (GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active.sick.extreme, RadiationMonitor.COMPARE_GTE_EXTREME).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, (GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active.sick.major, RadiationMonitor.COMPARE_GTE_MAJOR).ToggleAnims("anim_loco_radiation1_kanim", 4f).ToggleAnims("anim_idle_radiation1_kanim", 4f).ToggleExpression(Db.Get().Expressions.Radiation1).DefaultState(__instance.active.sick.minor.waiting);
                __instance.active.sick.minor.reacting.ToggleChore(new Func<RadiationMonitor.Instance, Chore>((smi) => createVomitChore.GetValue<Chore>(new object[] { smi })), __instance.active.sick.minor.waiting);
                __instance.active.sick.major.ToggleEffect(RadiationMonitor.majorSicknessEffect).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, __instance.active.sick.deadly, RadiationMonitor.COMPARE_GTE_DEADLY).ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, (GameStateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State)__instance.active.sick.extreme, RadiationMonitor.COMPARE_GTE_EXTREME).ToggleAnims("anim_loco_radiation2_kanim", 4f).ToggleAnims("anim_idle_radiation2_kanim", 4f).ToggleExpression(Db.Get().Expressions.Radiation2).DefaultState(__instance.active.sick.major.waiting);
                __instance.active.sick.major.waiting.ScheduleGoTo(120f, (StateMachine.BaseState)__instance.active.sick.major.vomiting);
                __instance.active.sick.major.vomiting.ToggleChore(new Func<RadiationMonitor.Instance, Chore>((smi) => createVomitChore.GetValue<Chore>(new object[] { smi })), __instance.active.sick.major.waiting);
                __instance.active.sick.extreme.ParamTransition<float>((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.Parameter<float>)__instance.radiationExposure, __instance.active.sick.deadly, RadiationMonitor.COMPARE_GTE_DEADLY).ToggleEffect(RadiationMonitor.extremeSicknessEffect).ToggleAnims("anim_loco_radiation3_kanim", 4f).ToggleAnims("anim_idle_radiation3_kanim", 4f).ToggleExpression(Db.Get().Expressions.Radiation3).DefaultState(__instance.active.sick.extreme.waiting);
                __instance.active.sick.extreme.waiting.ScheduleGoTo(60f, (StateMachine.BaseState)__instance.active.sick.extreme.vomiting);
                __instance.active.sick.extreme.vomiting.ToggleChore(new Func<RadiationMonitor.Instance, Chore>((smi) => createVomitChore.GetValue<Chore>(new object[] { smi })), __instance.active.sick.extreme.waiting);
                __instance.active.sick.deadly.ToggleAnims("anim_loco_radiation4_kanim", 4f).ToggleAnims("anim_idle_radiation4_kanim", 4f).ToggleExpression(Db.Get().Expressions.Radiation4).Enter((StateMachine<RadiationMonitor, RadiationMonitor.Instance, IStateMachineTarget, object>.State.Callback)(smi => smi.GetComponent<Health>().Incapacitate(GameTags.RadiationSicknessIncapacitation)));

                return false;
            }

        }
    }
}
