using UnityEngine;

namespace HysteresisStorage.UI
{
    internal class HysteresisStorageSideScreen : SideScreenContent
    {
        private IUserControlledCapacity target;
        private HysteresisStorageLogic targetLogicComponent;

        private LocText inputLabel;
        private KSlider slider;
        private KNumberInputField numberInput;
        private LocText unitsLabel;

        public event System.Action<float> OnValueChangedEvent = delegate { };

        private void SetElements()
        {
            this.inputLabel = transform.Find("ValidContent/Max/Label").GetComponent<LocText>();
            this.slider = transform.Find("ValidContent/SliderContainer/Slider").GetComponent<KSlider>();
            this.numberInput = transform.Find("ValidContent/Max/VerticalLayout/InputField").GetComponent<KNumberInputField>();
            this.unitsLabel = transform.Find("ValidContent/Max/UnitsLabel").GetComponent<LocText>();
        }
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            SetElements();
        }
        protected override void OnSpawn()
        {
            base.OnSpawn();

            inputLabel.text = "Min:";
            unitsLabel.text = target.CapacityUnits;
            slider.SetTooltipText(string.Format(HysteresisStorage.UI.STRINGS.HYSTERESISSTORAGESIDESCREEN.SLIDER_TOOLTIP, 0, target.CapacityUnits));
            slider.onDrag += delegate
            {
                ReceiveValueFromSlider(slider.value);
            };
            slider.onPointerDown += delegate
            {
                ReceiveValueFromSlider(slider.value);
            };
            slider.onMove += delegate
            {
                ReceiveValueFromSlider(slider.value);
            };
            numberInput.onEndEdit += delegate
            {
                ReceiveValueFromInput(numberInput.currentValue);
            };
            numberInput.decimalPlaces = 1;
        }

        public override bool IsValidForTarget(GameObject target)
        {
            var targetComponent = target.GetComponent<HysteresisStorageLogic>();
            return !targetComponent.IsNullOrDestroyed();
        }

        public override void SetTarget(GameObject new_target)
        {
            this.gameObject.SetActive(true);
            if (new_target == null)
            {
                Debug.LogError("Invalid gameObject received");
                return;
            }

            targetLogicComponent = new_target.GetComponent<HysteresisStorageLogic>();
            if (targetLogicComponent == null)
            {
                Debug.LogError("The gameObject received does not contain a HysteresisStorageLogicComponenet component");
                return;
            }

            ToggleContent(targetLogicComponent.HysteresisEnabled);

            target = new_target.GetComponent<IUserControlledCapacity>();

            var maxValue = Mathf.Max(0, target.UserMaxCapacity - 1);
            var value = Mathf.Min(slider.maxValue, targetLogicComponent.MinUserStorage);
            slider.minValue = 0;
            slider.maxValue = maxValue;
            slider.value = value;
            slider.SetTooltipText(string.Format(HysteresisStorage.UI.STRINGS.HYSTERESISSTORAGESIDESCREEN.SLIDER_TOOLTIP, slider.value, target.CapacityUnits));

            unitsLabel.text = target.CapacityUnits;
            numberInput.minValue = 0;
            numberInput.maxValue = slider.maxValue;
            numberInput.currentValue = slider.value;
            numberInput.SetDisplayValue(slider.value.ToString());
            numberInput.Activate();

            targetLogicComponent.MinUserStorage = value;

            targetLogicComponent.OnCapacityChangedEvent += RefreshSliderRange;
            targetLogicComponent.GetComponent<ToggleButton>().OnHysteresisToggleEvent += ToggleContent;
        }

        public override void ClearTarget()
        {
            targetLogicComponent.OnCapacityChangedEvent -= RefreshSliderRange;
            targetLogicComponent.GetComponent<ToggleButton>().OnHysteresisToggleEvent -= ToggleContent;

            base.ClearTarget();
        }

        private void ReceiveValueFromSlider(float newValue)
        {
            UpdateHysteresisThreshold(newValue);
        }

        private void ReceiveValueFromInput(float newValue)
        {
            UpdateHysteresisThreshold(newValue);
        }

        private void UpdateHysteresisThreshold(float newValue)
        {
            slider.value = newValue;
            slider.SetTooltipText(string.Format(HysteresisStorage.UI.STRINGS.HYSTERESISSTORAGESIDESCREEN.SLIDER_TOOLTIP, slider.value, target.CapacityUnits));

            numberInput.currentValue = newValue;
            numberInput.SetDisplayValue(slider.value.ToString());

            targetLogicComponent.MinUserStorage = newValue;
        }

        private void RefreshSliderRange(float newMaxCapacity)
        {
            if (!targetLogicComponent.HysteresisEnabled)
                return;

            var maxValue = Mathf.Max(0, newMaxCapacity - 1);
            var value = Mathf.Min(maxValue, slider.value);

            slider.value = value;
            numberInput.currentValue = value;

            numberInput.maxValue = maxValue;
            slider.maxValue = maxValue;

            numberInput.SetDisplayValue(value.ToString());

            targetLogicComponent.MinUserStorage = value;
        }


        public void ToggleContent(bool isEnabled)
        {
            transform.Find("ValidContent").gameObject.SetActive(isEnabled);
        }
    }
}