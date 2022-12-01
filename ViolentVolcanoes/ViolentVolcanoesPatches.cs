using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ViolentVolcanoes
{
    public class ViolentVolcanoesPatches
    {
        [HarmonyPatch(declaringType: typeof(Geyser.States))]
        [HarmonyPatch(nameof(Geyser.States.InitializeStates))]
        public class GeyserStates_InitializeStates_Patch
        {
            private const float DEG2RAD = Mathf.PI / 180f;
            private const int TEMPERATURE_RANGE = 20;

            // Comet count per burst
            private const int COMET_COUNT = 3;
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var code = new List<CodeInstruction>(instructions);

                int insertionIndex = -1;
                Label preEruptLabel = il.DefineLabel();
                for (int i = 1; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Callvirt && code[i - 1].opcode == OpCodes.Ldfld && code[i - 1].operand is FieldInfo f && f == AccessTools.Field(typeof(Geyser.States.EruptState), nameof(Geyser.States.erupt)))
                    {
                        insertionIndex = i;
                        code[i].labels.Add(preEruptLabel);
                        break;
                    }
                }

                var instructionsToInsert = new List<CodeInstruction>();

                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ret));

                if (insertionIndex != -1)
                {
                    code.InsertRange(insertionIndex, instructionsToInsert);
                }

                return code;
            }

            static void Postfix(Geyser.States __instance)
            {
                var emitterField = typeof(Geyser).GetField("emitter", BindingFlags.Instance | BindingFlags.NonPublic);

                __instance.erupt.DefaultState(__instance.erupt.erupting)
                .ScheduleGoTo((Geyser.StatesInstance smi) => smi.master.RemainingEruptTime(), __instance.post_erupt)
                .Enter(delegate (Geyser.StatesInstance smi)
                {
                    ElementEmitter emitter = (ElementEmitter)emitterField.GetValue(smi.master);
                    emitter.SetEmitting(true);
                })
                .Update((smi, dt) =>
                {
                    if (smi.master.configuration.typeId == GeyserGenericConfig.SmallVolcano || smi.master.configuration.typeId == GeyserGenericConfig.BigVolcano && smi.GetComponent<ElementEmitter>().isEmitterBlocked == false)
                    {
                        // Start violence
                        Burst(smi);
                    }
                }, UpdateRate.SIM_1000ms)
                .Exit(delegate (Geyser.StatesInstance smi)
                {
                    ElementEmitter emitter = (ElementEmitter)emitterField.GetValue(smi.master);
                    emitter.SetEmitting(false);
                });

                __instance.erupt.erupting.EventTransition(GameHashes.EmitterBlocked, __instance.erupt.overpressure, (Geyser.StatesInstance smi) => smi.GetComponent<ElementEmitter>().isEmitterBlocked).PlayAnim("erupt", KAnim.PlayMode.Loop);
                __instance.erupt.overpressure.EventTransition(GameHashes.EmitterUnblocked, __instance.erupt.erupting, (Geyser.StatesInstance smi) => !smi.GetComponent<ElementEmitter>().isEmitterBlocked).ToggleMainStatusItem(Db.Get().MiscStatusItems.SpoutOverPressure).PlayAnim("inactive", KAnim.PlayMode.Loop);
                __instance.post_erupt.PlayAnim("shake", KAnim.PlayMode.Loop).ToggleMainStatusItem(Db.Get().MiscStatusItems.SpoutIdle).ScheduleGoTo((Geyser.StatesInstance smi) => smi.master.RemainingEruptPostTime(), __instance.idle);
            }

            static void Burst(Geyser.StatesInstance smi)
            {
                float volcanoTemp = smi.master.configuration.GetTemperature();
                float eruptionRate = smi.master.configuration.GetEmitRate();

                for (int i = 0; i < COMET_COUNT; i++)
                {
                    GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(MagmaCometConfig.ID), smi.master.transform.position + (Vector3.up * 4f), Quaternion.identity);
                    gameObject.SetActive(true);

                    Comet comet = gameObject.GetComponent<Comet>();
                    comet.massRange = new Vector2(eruptionRate * 0.2f, eruptionRate * 0.3f);
                    comet.temperatureRange = new Vector2(volcanoTemp - TEMPERATURE_RANGE, volcanoTemp + TEMPERATURE_RANGE);
                    comet.ignoreObstacleForDamage.Set(smi.master.gameObject.GetComponent<KPrefabID>());
                    if (!ViolentVolcanoesMod.Config.CreateDiamondTiles)
                        comet.addTiles = 0;


                    PrimaryElement primaryElement = comet.gameObject.AddOrGet<PrimaryElement>();
                    primaryElement.Temperature = volcanoTemp;

                    float angle = UnityEngine.Random.Range(30f + (i * 40f) - (i * 10f), 30f + ((i + 1) * 40f) + ((COMET_COUNT - i) * 10f));
                    float rad = angle * Mathf.Deg2Rad;
                    comet.Velocity = new Vector2((0f - Mathf.Cos(rad)) * 12f, Mathf.Sin(rad) * 14f);
                    comet.GetComponent<KBatchedAnimController>().Rotation = (-angle) - 90f;
                }
            }
        }
    }
}
