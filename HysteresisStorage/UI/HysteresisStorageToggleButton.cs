using KSerialization;
using STRINGS;
using UnityEngine;

namespace HysteresisStorage
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ToggleButton : KMonoBehaviour, ISaveLoadable, IToggleHandler, IEventDispose
    {
        [Serialize]
        private bool isComponentEnabled = false;

        public bool IsEnabled
        {
            get
            {
                return isComponentEnabled;
            }
            set
            {
                isComponentEnabled = value;

                OnHysteresisToggleEvent?.Invoke(value);
            }
        }

        public event System.Action<bool> OnHysteresisToggleEvent;

        private static readonly EventSystem.IntraObjectHandler<ToggleButton> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<ToggleButton>(delegate (ToggleButton component, object data)
        {
            component.OnRefreshUserMenu(data);
        });

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // IsEnabled = toggleEnabled;
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.RefreshUserMenu);
            ((IEventDispose)this).UnregisterAllDelegates();

            base.OnCleanUp();
        }

        public void HandleToggle() { }

        public bool IsHandlerOn()
        {
            return IsEnabled;
        }

        private void OnMenuToggle()
        {
            IsEnabled = !IsEnabled;
        }

        private void OnRefreshUserMenu(object data)
        {
            bool isEnabled = IsEnabled;
            KIconButtonMenu.ButtonInfo buttonInfo = null;
            buttonInfo = !isEnabled
            ? new KIconButtonMenu.ButtonInfo("", HysteresisStorage.UI.STRINGS.USERMENUACTIONS.NAME_OFF, OnMenuToggle, Action.NumActions, null, null, null, HysteresisStorage.UI.STRINGS.USERMENUACTIONS.TOOLTIP_OFF)
            : new KIconButtonMenu.ButtonInfo("action_building_disabled", HysteresisStorage.UI.STRINGS.USERMENUACTIONS.NAME, OnMenuToggle, Action.NumActions, null, null, null, HysteresisStorage.UI.STRINGS.USERMENUACTIONS.TOOLTIP);
            Game.Instance.userMenu.AddButton(base.gameObject, buttonInfo);
        }

        void IEventDispose.UnregisterAllDelegates()
        {
            System.Delegate[] delegates = this.OnHysteresisToggleEvent?.GetInvocationList();
            if (delegates != null && delegates.Length > 0)
            {
                foreach (System.Delegate del in delegates)
                {
                    System.Action<bool> customHandler = (System.Action<bool>)del;
                    this.OnHysteresisToggleEvent -= customHandler;
                }
            }
        }
    }
}
