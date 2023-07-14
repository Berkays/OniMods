using System;
using System.Reflection;
using KSerialization;
using UnityEngine;

namespace HysteresisStorage
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class HysteresisStorageLogic : KMonoBehaviour, IEventDispose
    {
        [Serialize]
        public bool onceFull = false;

        [Serialize]
        private float minUserStorage = 0;

        private float lastMaxCapacity = -1;

        public float MinUserStorage
        {
            get
            {
                return this.minUserStorage;
            }
            set
            {
                bool triggerRefresh = this.minUserStorage != value;
                this.minUserStorage = value;

                if (triggerRefresh)
                    RefreshFilter();
            }
        }

        public bool HysteresisEnabled
        {
            get
            {
                return this.toggleButton.IsEnabled;
            }
        }

        [MyCmpGet]
        private Storage storage;

        private IUserControlledCapacity userControlledCapacity;

        private FilteredStorage filteredStorage;

        [MyCmpGet]
        public CopyBuildingSettings copyBuildingSettings;

        [MyCmpGet]
        private ToggleButton toggleButton;

        private static readonly EventSystem.IntraObjectHandler<HysteresisStorageLogic> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<HysteresisStorageLogic>(OnCopySettings);

        public event System.Action<float> OnCapacityChangedEvent = delegate { };

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            this.userControlledCapacity = this.storage.GetComponent<IUserControlledCapacity>();

            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            this.toggleButton.OnHysteresisToggleEvent += onToggle;

            Component componentInstance = GetDerivedType();
            Type componentType = componentInstance.GetType();

            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = componentType.GetField("filteredStorage", bindingFlags);
            this.filteredStorage = (FilteredStorage)field?.GetValue(componentInstance);

            if (this.filteredStorage == null)
            {
                this.filteredStorage = (FilteredStorage)field?.GetValue(this.storage.gameObject.GetComponent<StorageLocker>());
            }
        }

        protected override void OnCleanUp()
        {
            ((IEventDispose)this).UnregisterAllDelegates();

            base.OnCleanUp();
        }

        void IEventDispose.UnregisterAllDelegates()
        {
            System.Delegate[] delegates = this.OnCapacityChangedEvent?.GetInvocationList();
            if (delegates != null && delegates.Length > 0)
            {
                foreach (System.Delegate del in delegates)
                {
                    System.Action<float> customHandler = (System.Action<float>)del;
                    this.OnCapacityChangedEvent -= customHandler;
                }
            }
        }

        private static void OnCopySettings(HysteresisStorageLogic comp, object data)
        {
            comp.OnCopySettings(data);
        }

        internal void OnCopySettings(object data)
        {
            HysteresisStorageLogic comp = ((UnityEngine.GameObject)data).GetComponent<HysteresisStorageLogic>();
            if (comp != null)
            {
                this.minUserStorage = comp.MinUserStorage;
                this.toggleButton.IsEnabled = comp.toggleButton.IsEnabled;
            }
        }

        private Component GetDerivedType()
        {
            var type = this.storage.GetComponent<RationBox>()?.GetType(); // Has Storage, IUser, FilteredStorage
            if (type == null)
                type = this.storage.GetComponent<StorageLocker>()?.GetType(); // Has IUser, FilteredStorage
            if (type == null)
                type = this.storage.GetComponent<Refrigerator>()?.GetType();  // Has Storage, IUser, FilteredStorage
            if (type == null)
                type = this.storage.GetComponent<StorageLockerSmart>()?.GetType(); // Has none/ Base has storagelocker
            if (type == null)
                type = this.storage.GetComponent<CreatureFeeder>()?.GetType(); // Has none/ Base has storagelocker
            if (type == null)
                type = this.storage.GetComponent<SolidConduitInbox>()?.GetType(); // Has FilteredStorage

            return this.storage.GetComponent(type);
        }

        private void onToggle(bool enabled)
        {
            RefreshFilter();
        }

        public void RefreshFilter()
        {
            if (this.filteredStorage != null)
                this.filteredStorage.FilterChanged();
        }

        public void ForceCapacityChangeRefresh()
        {
            if (userControlledCapacity != null && userControlledCapacity.UserMaxCapacity != this.lastMaxCapacity)
            {
                // Reset hysteresis state
                this.onceFull = false;

                this.lastMaxCapacity = userControlledCapacity.UserMaxCapacity;
                OnCapacityChangedEvent?.Invoke(userControlledCapacity.UserMaxCapacity);
            }
        }
    }
}