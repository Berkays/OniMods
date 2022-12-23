using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ConfigurableTimers
{
    internal class ConfigurableTimersPatches
    {
        private static LogicTimerSensor targetTimer;
        private static KNumberInputField numberField;
        private static KSlider slider;

        private static bool isEditing = false;

        private static readonly Color offColor = new Color32(250, 110, 110, 255);
        private static readonly Color onColor = new Color32(140, 230, 140, 255);
        private static int tickCounter = 0;


        [HarmonyPatch(typeof(TimerSideScreen), "OnPrefabInit")]
        static class TimerSideScreen_OnPrefabInit_Patch
        {
            internal static void Postfix(TimerSideScreen __instance)
            {
                GameObject label = __instance.gameObject.transform.Find("Contents/RedDurationLabel").gameObject;
                GameObject currentTimeLabel = GameObject.Instantiate(label, label.transform.parent);
                currentTimeLabel.name = "CurrentTimeLabel";
                currentTimeLabel.transform.SetSiblingIndex(label.transform.GetSiblingIndex() + 2);
                currentTimeLabel.GetComponent<LocText>().SetText("Current Time");

                GameObject original = __instance.gameObject.transform.Find("Contents/RedDurationSliderContainer").gameObject;
                GameObject currentTimeObject = GameObject.Instantiate(original, original.transform.parent);
                currentTimeObject.name = "TimerContainer";
                currentTimeObject.transform.SetSiblingIndex(original.transform.GetSiblingIndex() + 2);

                numberField = currentTimeObject.transform.Find("NumberInputField").gameObject.GetComponent<KNumberInputField>();
                slider = currentTimeObject.transform.Find("Slider").gameObject.GetComponent<KSlider>();

                numberField.decimalPlaces = 1;
                numberField.minValue = 0;
                numberField.maxValue = 600f;

                slider.minValue = 0;
                slider.maxValue = 600f;
                slider.wholeNumbers = false;
            }
        }

        [HarmonyPatch(typeof(TimerSideScreen), "OnSpawn")]
        static class TimerSideScreen_OnSpawn_Patch
        {
            internal static void Postfix(LogicTimerSensor ___targetTimedSwitch)
            {
                targetTimer = ___targetTimedSwitch;

                numberField.onStartEdit += () =>
                {
                    isEditing = true;
                };
                numberField.onEndEdit += () =>
                {
                    isEditing = false;
                    targetTimer.timeElapsedInCurrentState = numberField.currentValue;
                };
            }
        }

        [HarmonyPatch(typeof(TimerSideScreen), "UpdateVisualsForNewTarget")]
        static class TimerSideScreen_UpdateVisualsForNewTarget_Patch
        {
            delegate void SetStateDelegate(Switch instance, bool state);
            delegate void OnSwitchToggledDelegate(LogicTimerSensor instance, bool state);
            private static readonly SetStateDelegate setStateMethod = (SetStateDelegate)typeof(Switch).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(SetStateDelegate));
            private static readonly OnSwitchToggledDelegate onSwitchToggleMethod = (OnSwitchToggledDelegate)typeof(LogicTimerSensor).GetMethod("OnSwitchToggled", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(OnSwitchToggledDelegate));

            internal static void Postfix(LogicTimerSensor ___targetTimedSwitch, KNumberInputField ___onDurationNumberInput)
            {
                slider.onValueChanged.RemoveAllListeners();
                targetTimer = ___targetTimedSwitch;

                float durationSum = ___targetTimedSwitch.onDuration + ___targetTimedSwitch.offDuration;
                slider.maxValue = durationSum;

                if (___targetTimedSwitch.displayCyclesMode)
                    numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState / 600f);
                else
                    numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState);

                slider.value = ___targetTimedSwitch.IsSwitchedOn ? ___targetTimedSwitch.timeElapsedInCurrentState : ___targetTimedSwitch.timeElapsedInCurrentState + ___onDurationNumberInput.currentValue;

                numberField.Activate();
                slider.onValueChanged.AddListener(delegate
                {
                    float onDuration = ___targetTimedSwitch.displayCyclesMode ? ___onDurationNumberInput.currentValue * 600f : ___onDurationNumberInput.currentValue;

                    float newVal = slider.value;
                    if (___targetTimedSwitch.IsSwitchedOn)
                    {
                        if (slider.value > onDuration)
                        {
                            // Switch off
                            setStateMethod(___targetTimedSwitch, false);
                            onSwitchToggleMethod(___targetTimedSwitch, false);
                            newVal -= onDuration;
                        }
                    }
                    else
                    {
                        newVal -= onDuration;
                        if (slider.value < onDuration)
                        {
                            // Switch on
                            setStateMethod(___targetTimedSwitch, true);
                            onSwitchToggleMethod(___targetTimedSwitch, true);
                            newVal += onDuration;
                        }
                    }

                    ___targetTimedSwitch.timeElapsedInCurrentState = newVal;
                    numberField.SetAmount(___targetTimedSwitch.displayCyclesMode ? ___targetTimedSwitch.timeElapsedInCurrentState / 600f : ___targetTimedSwitch.timeElapsedInCurrentState);
                });
            }
        }

        [HarmonyPatch(typeof(TimerSideScreen), "ToggleMode")]
        static class TimerSideScreen_ToggleMode_Patch
        {
            internal static void Postfix(LogicTimerSensor ___targetTimedSwitch)
            {
                if (___targetTimedSwitch.displayCyclesMode)
                {
                    numberField.decimalPlaces = 2;
                    numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState / 600f);

                    return;
                }

                numberField.decimalPlaces = 1;
                numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState);
            }
        }

        [HarmonyPatch(typeof(TimerSideScreen), "ChangeSetting")]
        static class TimerSideScreen_ChangeSetting_Patch
        {
            internal static void Postfix(LogicTimerSensor ___targetTimedSwitch)
            {
                slider.maxValue = ___targetTimedSwitch.onDuration + ___targetTimedSwitch.offDuration;
                if (___targetTimedSwitch.displayCyclesMode)
                {
                    numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState / 600f);
                    return;
                }

                numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState);
            }
        }

        [HarmonyPatch(typeof(TimerSideScreen))]
        [HarmonyPatch(nameof(TimerSideScreen.RenderEveryTick))]
        static class TimerSideScreen_RenderEveryTick_Patch
        {

            internal static void Postfix(LogicTimerSensor ___targetTimedSwitch, KNumberInputField ___onDurationNumberInput)
            {
                // Prevent excessive updates
                if (tickCounter >= 30)
                {
                    var textField = numberField.GetComponent<Image>();
                    if (___targetTimedSwitch.IsSwitchedOn)
                        textField.color = onColor;
                    else
                        textField.color = offColor;

                    tickCounter = 0;
                }

                tickCounter++;


                if (___targetTimedSwitch.displayCyclesMode)
                {
                    if (!isEditing)
                        numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState / 600f);
                    slider.value = ___targetTimedSwitch.IsSwitchedOn ? ___targetTimedSwitch.timeElapsedInCurrentState : ___targetTimedSwitch.timeElapsedInCurrentState + ___onDurationNumberInput.currentValue * 600f;

                    return;
                }

                if (!isEditing)
                    numberField.SetAmount(___targetTimedSwitch.timeElapsedInCurrentState);
                slider.value = ___targetTimedSwitch.IsSwitchedOn ? ___targetTimedSwitch.timeElapsedInCurrentState : ___targetTimedSwitch.timeElapsedInCurrentState + ___onDurationNumberInput.currentValue;
            }
        }
    }
}