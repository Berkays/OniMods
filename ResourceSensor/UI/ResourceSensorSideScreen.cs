using System;
using UnityEngine;
using UnityEngine.UI;

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

        private GameObject countStorageContainer;
        public KToggle countStorageToggle;

        public KImage distanceCheckmark;

        public KImage roomCheckmark;

        public KImage globalCheckmark;

        public KImage countStorageCheckmark;

        private GameObject dividerMargin;
        private GameObject divider;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            SetElements();
        }

        private void SetElements()
        {
            GameObject distanceContainer = transform.Find("Contents/CheckboxGroup").gameObject;

            // Global UI
            GameObject globalContainer = GameObject.Instantiate(distanceContainer, distanceContainer.transform.parent);
            Transform checkBoxGroup = globalContainer.transform.Find("CrittersCheckBox");
            checkBoxGroup.name = "CountGlobalCheckBox";

            globalContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Global Mode");
            countGlobalToggle = checkBoxGroup.GetComponent<KToggle>();
            globalCheckmark = countGlobalToggle.transform.Find("CheckMark").GetComponent<KImage>();

            // Distance UI
            checkBoxGroup = distanceContainer.transform.Find("CrittersCheckBox");
            checkBoxGroup.name = "CountDistanceCheckBox";

            distanceContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Distance Mode");
            countDistanceToggle = checkBoxGroup.GetComponent<KToggle>();
            distanceCheckmark = countDistanceToggle.transform.Find("CheckMark").GetComponent<KImage>();

            // Room UI
            GameObject roomContainer = transform.Find("Contents/CheckboxGroup/EggsCheckBox").parent.gameObject;
            checkBoxGroup = roomContainer.transform.Find("EggsCheckBox");
            checkBoxGroup.name = "CountRoomCheckBox";

            roomContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Room Mode");
            countRoomToggle = checkBoxGroup.GetComponent<KToggle>();
            roomCheckmark = countRoomToggle.transform.Find("CheckMark").GetComponent<KImage>();

            // Include Storage UI
            countStorageContainer = GameObject.Instantiate(distanceContainer, distanceContainer.transform.parent);
            countStorageContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Include Storage Buildings");
            checkBoxGroup = countStorageContainer.transform.Find("CountDistanceCheckBox");
            checkBoxGroup.name = "IncludeStorageCheckBox";
            countStorageToggle = checkBoxGroup.GetComponent<KToggle>();
            countStorageCheckmark = countStorageToggle.transform.Find("CheckMark").GetComponent<KImage>();
            countStorageContainer.GetComponent<RectTransform>().localScale = new Vector3(0.94f, 0.94f, 1f);

            // Divider
            dividerMargin = new GameObject("Margin");
            dividerMargin.transform.SetParent(distanceContainer.transform.parent);
            dividerMargin.transform.SetSiblingIndex(4);
            dividerMargin.AddComponent<LayoutElement>();
            dividerMargin.transform.localScale = new Vector3(1f, 1f, 1f);
            dividerMargin.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 4f);

            divider = new GameObject("Divider");
            divider.transform.SetParent(distanceContainer.transform.parent);
            divider.transform.SetSiblingIndex(5);
            LayoutElement le = divider.AddComponent<LayoutElement>();
            le.minWidth = 240f;
            le.preferredWidth = 240f;
            le.minHeight = 1f;
            le.preferredHeight = 1f;
            le.flexibleWidth = 1;
            divider.transform.localScale = new Vector3(1f, 1f, 1f);
            divider.transform.localPosition = new Vector3(-122, -55, 0);

            divider.AddComponent<CanvasRenderer>();
            Image i = divider.AddComponent<Image>();
            i.color = new Color(0.7f, 0.7f, 0.7f);

            var tempSliderScreen = ResourceSensorPatches.tempSliderSideScreen;

            GameObject original = tempSliderScreen.transform.Find("ValidContent").gameObject;
            distanceSliderContainer = GameObject.Instantiate(original, distanceContainer.transform.parent);
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
            countStorageToggle.onClick += ToggleCountStorage;

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
            countStorageCheckmark.enabled = targetSensor.IncludeStorage;

            switch (targetSensor.Mode)
            {
                case LogicResourceSensor.SensorMode.Distance:
                    distanceCheckmark.enabled = true;
                    roomCheckmark.enabled = false;
                    globalCheckmark.enabled = false;
                    distanceSliderContainer.SetActive(true);
                    countStorageContainer.SetActive(true);
                    dividerMargin.SetActive(true);
                    divider.SetActive(true);
                    break;
                case LogicResourceSensor.SensorMode.Room:
                    distanceCheckmark.enabled = false;
                    roomCheckmark.enabled = true;
                    globalCheckmark.enabled = false;
                    countStorageContainer.SetActive(true);
                    distanceSliderContainer.SetActive(false);
                    dividerMargin.SetActive(true);
                    divider.SetActive(true);
                    break;
                case LogicResourceSensor.SensorMode.Global:
                    distanceCheckmark.enabled = false;
                    roomCheckmark.enabled = false;
                    globalCheckmark.enabled = true;
                    countStorageContainer.SetActive(false);
                    distanceSliderContainer.SetActive(false);
                    dividerMargin.SetActive(false);
                    divider.SetActive(false);
                    break;
            }
        }

        private void ToggleDistance()
        {
            targetSensor.Mode = LogicResourceSensor.SensorMode.Distance;
            distanceCheckmark.enabled = true;
            roomCheckmark.enabled = false;
            globalCheckmark.enabled = false;

            countStorageContainer.SetActive(true);
            distanceSliderContainer.SetActive(true);
            dividerMargin.SetActive(true);
            divider.SetActive(true);
        }
        private void ToggleRoom()
        {
            targetSensor.Mode = LogicResourceSensor.SensorMode.Room;
            distanceCheckmark.enabled = false;
            roomCheckmark.enabled = true;
            globalCheckmark.enabled = false;

            countStorageContainer.SetActive(true);
            distanceSliderContainer.SetActive(false);
            dividerMargin.SetActive(true);
            divider.SetActive(true);
        }
        private void ToggleGlobal()
        {
            targetSensor.Mode = LogicResourceSensor.SensorMode.Global;
            distanceCheckmark.enabled = false;
            roomCheckmark.enabled = false;
            globalCheckmark.enabled = true;

            countStorageContainer.SetActive(false);
            distanceSliderContainer.SetActive(false);
            dividerMargin.SetActive(false);
            divider.SetActive(false);
        }

        private void ToggleCountStorage()
        {
            targetSensor.IncludeStorage = !targetSensor.IncludeStorage;
            countStorageCheckmark.enabled = targetSensor.IncludeStorage;
        }

        private void updateDistance()
        {
            targetSensor.Distance = (int)distanceSlider.value;
            distanceText.SetText($"Distance: {targetSensor.Distance}");
            distanceSlider.SetTooltipText(string.Format(ResourceSensor.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, distanceSlider.value));
        }
    }
}