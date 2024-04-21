using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace ResourceSensor
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LogicResourceSensor : Switch, ISaveLoadable, IThresholdSwitch, ISim1000ms
    {
        public enum SensorMode
        {
            Distance = 0,
            Room = 1,
            Global = 2
        }

        private bool wasOn;

        [Serialize]
        private SensorMode mode = SensorMode.Distance;

        public SensorMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (this.mode != value)
                {
                    this.mode = value;
                    if (this.mode != SensorMode.Distance)
                    {
                        this.visualizer.visCells.Clear();
                        this.visualizer.Refresh();
                    }
                }
            }
        }

        [Serialize]
        private int distance = 3;

        public int Distance
        {
            get
            {
                return this.distance;
            }
            set
            {
                if (this.distance != value)
                {
                    this.distance = value;
                    visualiserDirty = true;
                }
            }
        }

        [Serialize]
        private bool includeStorage = false;

        public bool IncludeStorage
        {
            get
            {
                return this.includeStorage;
            }
            set
            {
                this.includeStorage = value;
            }
        }

#pragma warning disable CS0649
#pragma warning disable CS0169
        [MyCmpGet]
        private DistanceVisualizer visualizer;
        private bool visualiserDirty = false;

        [MyCmpGet]
        private TreeFilterable treeFilterable;

        [MyCmpGet]
        private LogicPorts logicPorts;

        [Serialize]
        public float countThreshold;

        [Serialize]
        public bool activateOnGreaterThan = true;

        private float currentCount;

        private KSelectable selectable;

        private Guid roomStatusGUID;

        private KBatchedAnimController animController;

        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;
#pragma warning restore CS0649
#pragma warning restore CS0169

        private static readonly EventSystem.IntraObjectHandler<LogicResourceSensor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<LogicResourceSensor>(delegate (LogicResourceSensor component, object data)
        {
            component.OnCopySettings(data);
        });

        public float Threshold
        {
            get
            {
                return countThreshold;
            }
            set
            {
                countThreshold = value;
            }
        }

        public bool ActivateAboveThreshold
        {
            get
            {
                return activateOnGreaterThan;
            }
            set
            {
                activateOnGreaterThan = value;
            }
        }

        public float CurrentValue => currentCount;

        public float RangeMin => 0f;

        public float RangeMax => 100000f;

        public LocString Title => UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.TITLE;

        public LocString ThresholdValueName => UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.VALUE_NAME;

        public string AboveToolTip => UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.RESOURCE_TOOLTIP_ABOVE;

        public string BelowToolTip => UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.RESOURCE_TOOLTIP_BELOW;

        public ThresholdScreenLayoutType LayoutType => ThresholdScreenLayoutType.SliderBar;

        public int IncrementScale => 100;

        public NonLinearSlider.Range[] GetRanges => NonLinearSlider.GetDefaultRange(RangeMax);

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            selectable = GetComponent<KSelectable>();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        private void OnCopySettings(object data)
        {
            LogicResourceSensor component = ((GameObject)data).GetComponent<LogicResourceSensor>();
            if (component != null)
            {
                countThreshold = component.countThreshold;
                activateOnGreaterThan = component.activateOnGreaterThan;
                Mode = component.Mode;
                IncludeStorage = component.IncludeStorage;
                Distance = component.Distance;
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            base.OnToggle += OnSwitchToggled;
            animController = GetComponent<KBatchedAnimController>();
            UpdateLogicCircuit();
            UpdateVisualState(force: true);
            wasOn = switchedOn;
            visualiserDirty = true;
        }

        public void Sim1000ms(float dt)
        {
            bool state;

            if (treeFilterable.AcceptedTags.Count == 0)
            {
                currentCount = 0;
                state = activateOnGreaterThan ? (0 > countThreshold) : (0 < countThreshold);
                SetState(state);

                if (this.visualizer.visCells.Count > 0)
                {
                    this.visualizer.visCells.Clear();
                    this.visualizer.Refresh();
                }

                return;
            }

            if (this.mode == SensorMode.Room)
            {
                Room roomOfGameObject = Game.Instance.roomProber.GetRoomOfGameObject(base.gameObject);

                if (roomOfGameObject != null)
                {
                    currentCount = CountRoom(roomOfGameObject);
                    state = activateOnGreaterThan ? (currentCount > countThreshold) : (currentCount < countThreshold);

                    if (selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom))
                        selectable.RemoveStatusItem(roomStatusGUID);

                    SetState(state);

                    return;
                }

                if (selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom) == false)
                    roomStatusGUID = selectable.AddStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom);

                currentCount = 0;
                state = activateOnGreaterThan ? (0 > countThreshold) : (0 < countThreshold);
                SetState(state);

                return;
            }

            if (selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom))
                selectable.RemoveStatusItem(roomStatusGUID);

            if (this.mode == SensorMode.Distance)
            {
                currentCount = CountDistance();
                state = activateOnGreaterThan ? (currentCount > countThreshold) : (currentCount < countThreshold);
                SetState(state);

                return;
            }

            if (this.mode == SensorMode.Global)
            {
                currentCount = CountGlobal();
                state = activateOnGreaterThan ? (currentCount > countThreshold) : (currentCount < countThreshold);

                SetState(state);
            }
        }

        private void OnSwitchToggled(bool toggled_on)
        {
            UpdateLogicCircuit();
            UpdateVisualState();
        }

        private void UpdateLogicCircuit()
        {
            logicPorts.SendSignal(LogicSwitch.PORT_ID, switchedOn ? 1 : 0);
        }

        private void UpdateVisualState(bool force = false)
        {
            if (!(wasOn != switchedOn || force))
                return;

            wasOn = switchedOn;

            if (switchedOn)
            {
                animController.Play("Working_pre");
                animController.Queue("Working_loop", KAnim.PlayMode.Loop);
                return;
            }

            animController.Play("Working_pst");
            animController.Queue("off");
        }

        protected override void UpdateSwitchStatus()
        {
            StatusItem status_item = switchedOn ? Db.Get().BuildingStatusItems.LogicSensorStatusActive : Db.Get().BuildingStatusItems.LogicSensorStatusInactive;
            GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Power, status_item);
        }

        public float GetRangeMinInputField()
        {
            return RangeMin;
        }

        public float GetRangeMaxInputField()
        {
            return RangeMax;
        }

        public string Format(float value, bool units)
        {
            return $"{value} kg";
        }

        public float ProcessedSliderValue(float input)
        {
            return Mathf.RoundToInt(input);
        }

        public float ProcessedInputValue(float input)
        {
            int rnd = Mathf.CeilToInt(input);
            float actual = Mathf.Round(input * 100) / 100.0f;
            if (Mathf.Abs(actual - rnd) <= 0.01f)
                return rnd;

            return actual;
        }

        public LocString ThresholdValueUnits()
        {
            return "";
        }

        private float CountDistance()
        {
            int cell = logicPorts.GetPortCell(LogicSwitch.PORT_ID);

            this.visualizer.visCells.Clear();

            if (this.Distance == 0)
            {
                if (visualiserDirty && selectable.IsSelected)
                    this.visualizer.Refresh();
                return CountCell(cell);
            }

            HashSet<GameObject> countedBuildings = new HashSet<GameObject>();

            Grid.CellToXY(cell, out int cellX, out int cellY);

            int minX = cellX - this.Distance;
            int maxX = cellX + this.Distance;
            int minY = cellY - this.Distance;
            int maxY = cellY + this.Distance;

            float totalMass = 0;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    int searchCell = Grid.XYToCell(x, y);

                    if (Grid.IsSolidCell(searchCell))
                        continue;

                    this.visualizer.visCells.Add(searchCell);
                    totalMass += CountCell(searchCell);

                    if (this.includeStorage)
                    {
                        GameObject obj = Grid.Objects[searchCell, (int)ObjectLayer.Building];

                        if (obj == null || countedBuildings.Contains(obj))
                            continue;

                        countedBuildings.Add(obj);
                        totalMass += CountBuilding(obj);
                    }
                }
            }

            if (visualiserDirty && selectable.IsSelected)
                this.visualizer.Refresh();

            return totalMass;
        }

        private float CountRoom(Room room)
        {
            float totalMass = 0f;

            int minX = room.cavity.minX;
            int maxX = room.cavity.maxX;
            int minY = room.cavity.minY;
            int maxY = room.cavity.maxY;

            HashSet<GameObject> countedBuildings = new HashSet<GameObject>();

            RoomProber roomProber = Game.Instance.roomProber;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    int cell = Grid.XYToCell(x, y);

                    if (Grid.IsSolidCell(cell) || roomProber.GetCavityForCell(cell) != room.cavity)
                        continue;

                    totalMass += CountCell(cell);

                    if (this.includeStorage)
                    {
                        GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Building];

                        if (obj == null || countedBuildings.Contains(obj))
                            continue;

                        countedBuildings.Add(obj);
                        totalMass += CountBuilding(obj);
                    }
                }
            }

            return totalMass;
        }

        private float CountGlobal()
        {
            var tags = treeFilterable.AcceptedTags;

            WorldInventory worldInventory = gameObject.GetMyWorld().worldInventory;

            float totalMass = 0f;
            foreach (var tag in tags)
                totalMass += worldInventory.GetTotalAmount(tag, false);

            return totalMass;
        }

        private float CountCell(int cell)
        {
            var tags = treeFilterable.AcceptedTags;

            GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Pickupables];

            if (obj == null)
                return 0;

            float totalMass = 0f;

            ObjectLayerListItem objectLayerListItem = obj.GetComponent<Pickupable>().objectLayerListItem;
            while (objectLayerListItem != null)
            {
                GameObject obj2 = objectLayerListItem.gameObject;
                objectLayerListItem = objectLayerListItem.nextItem;

                if (obj2 != null && obj2.TryGetComponent<MinionIdentity>(out _) == false && obj2.TryGetComponent<KPrefabID>(out KPrefabID kPrefabID))
                {
                    foreach (var tag in tags)
                    {
                        if (kPrefabID.HasTag(tag))
                            totalMass += obj2.GetComponent<PrimaryElement>().Mass;
                    }
                }
            }

            return totalMass;
        }

        private float CountBuilding(GameObject obj)
        {
            if (obj.TryGetComponent(out BuildingUnderConstruction _))
                return 0;

            if (!obj.TryGetComponent(out StorageLocker _)
            && !obj.TryGetComponent(out StorageLockerSmart _)
            && !obj.TryGetComponent(out RationBox _)
            && !obj.TryGetComponent(out Refrigerator _))
                return 0;

            float totalMass = 0f;

            if (obj.TryGetComponent<Storage>(out Storage storage))
            {
                var tags = treeFilterable.AcceptedTags;

                foreach (var item in storage.items)
                {
                    foreach (var tag in tags)
                    {
                        if (item.HasTag(tag))
                        {
                            totalMass += item.GetComponent<PrimaryElement>().Mass;
                            break;
                        }
                    }
                }
            }

            return totalMass;
        }
    }
}
