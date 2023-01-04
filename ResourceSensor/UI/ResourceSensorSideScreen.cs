using UnityEngine;

namespace ResourceSensor
{
    public class ResourceSensorSideScreen : SideScreenContent
    {
        public LogicResourceSensor targetSensor;

        public KToggle countDistanceToggle;
        private GameObject distanceSliderContainer;
        public KSlider distanceSlider;
        public LocText distanceText;

        public KToggle countRoomToggle;

        public KToggle countGlobalToggle;

        public KImage distanceCheckmark;

        public KImage roomCheckmark;

        public KImage globalCheckmark;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            SetElements();
        }

        private void SetElements()
        {
            GameObject critterGroup = transform.Find("Contents/CheckboxGroup").gameObject;

            GameObject globalCheckbox = GameObject.Instantiate(critterGroup, critterGroup.transform.parent);
            globalCheckbox.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Global Mode");
            countGlobalToggle = globalCheckbox.transform.Find("CrittersCheckBox").GetComponent<KToggle>();
            globalCheckmark = countGlobalToggle.transform.Find("CheckMark").GetComponent<KImage>();

            critterGroup.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Distance Mode");
            countDistanceToggle = critterGroup.transform.Find("CrittersCheckBox").GetComponent<KToggle>();
            distanceCheckmark = countDistanceToggle.transform.Find("CheckMark").GetComponent<KImage>();

            GameObject eggGroup = transform.Find("Contents/CheckboxGroup/EggsCheckBox").parent.gameObject;
            eggGroup.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Room Mode");
            countRoomToggle = eggGroup.transform.Find("EggsCheckBox").GetComponent<KToggle>();
            roomCheckmark = countRoomToggle.transform.Find("CheckMark").GetComponent<KImage>();

            var tempSliderScreen = ResourceSensorPatches.tempSliderSideScreen;

            GameObject original = tempSliderScreen.transform.Find("ValidContent").gameObject;
            distanceSliderContainer = GameObject.Instantiate(original, critterGroup.transform.parent);
            distanceSliderContainer.name = "DistanceSlider";
            distanceSliderContainer.transform.SetSiblingIndex(2);

            distanceText = distanceSliderContainer.transform.Find("Max/Label").GetComponent<LocText>();
            distanceText.SetText("Distance: 3");
            distanceSliderContainer.transform.Find("Max/UnitsLabel").GetComponent<LocText>().SetText("Tiles");

            distanceSlider = distanceSliderContainer.transform.Find("SliderContainer/Slider").GetComponent<KSlider>();
            distanceSlider.wholeNumbers = true;
            distanceSlider.minValue = 0f;
            distanceSlider.value = 3;
            distanceSlider.maxValue = 20f;
            distanceSlider.SetTooltipText(string.Format(ResourceSensor.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, 3));

            GameObject.Destroy(distanceSliderContainer.transform.Find("Max/VerticalLayout").gameObject);
            GameObject.Destroy(tempSliderScreen.gameObject);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            countDistanceToggle.onClick += ToggleDistance;
            countRoomToggle.onClick += ToggleRoom;
            countGlobalToggle.onClick += ToggleGlobal;

            distanceSlider.SetTooltipText(string.Format(ResourceSensor.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, 0));
            distanceSlider.onDrag += updateDistance;
            distanceSlider.onPointerDown += updateDistance;
            distanceSlider.onMove += updateDistance;
        }

        public override bool IsValidForTarget(GameObject target)
        {
            var targetComponent = target.GetComponent<LogicResourceSensor>();
            return !targetComponent.IsNullOrDestroyed();
        }

        public override void SetTarget(GameObject target)
        {
            this.gameObject.SetActive(true);

            if (target == null)
            {
                Debug.LogError("Invalid gameObject received");
                return;
            }

            if (distanceCheckmark == null)
                SetElements();

            targetSensor = target.GetComponent<LogicResourceSensor>();
            if (targetSensor == null)
                return;

            distanceSlider.value = targetSensor.Distance;
            distanceText.SetText($"Distance: {targetSensor.Distance}");
            distanceSlider.SetTooltipText(string.Format(ResourceSensor.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, distanceSlider.value));
            distanceCheckmark.enabled = targetSensor.Mode == LogicResourceSensor.SensorMode.Distance;
            roomCheckmark.enabled = targetSensor.Mode == LogicResourceSensor.SensorMode.Room;
            globalCheckmark.enabled = targetSensor.Mode == LogicResourceSensor.SensorMode.Global;

            if (targetSensor.Mode == LogicResourceSensor.SensorMode.Distance)
            {
                distanceSliderContainer.SetActive(true);
                return;
            }

            distanceSliderContainer.SetActive(false);

        }

        private void ToggleDistance()
        {
            targetSensor.Mode = LogicResourceSensor.SensorMode.Distance;
            distanceCheckmark.enabled = true;
            roomCheckmark.enabled = false;
            globalCheckmark.enabled = false;

            distanceSliderContainer.SetActive(true);
        }
        private void ToggleRoom()
        {
            targetSensor.Mode = LogicResourceSensor.SensorMode.Room;
            distanceCheckmark.enabled = false;
            roomCheckmark.enabled = true;
            globalCheckmark.enabled = false;

            distanceSliderContainer.SetActive(false);
        }
        private void ToggleGlobal()
        {
            targetSensor.Mode = LogicResourceSensor.SensorMode.Global;
            distanceCheckmark.enabled = false;
            roomCheckmark.enabled = false;
            globalCheckmark.enabled = true;

            distanceSliderContainer.SetActive(false);
        }

        private void updateDistance()
        {
            targetSensor.Distance = (int)distanceSlider.value;
            distanceText.SetText($"Distance: {targetSensor.Distance}");
            distanceSlider.SetTooltipText(string.Format(ResourceSensor.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, distanceSlider.value));
        }
    }
}