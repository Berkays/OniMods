using System;
using KSerialization;

namespace AutomationBypass
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ToggleButton : KMonoBehaviour, ISaveLoadable, IToggleHandler
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
                if (value == isComponentEnabled)
                    return;
                isComponentEnabled = value;
            }
        }

        private static readonly EventSystem.IntraObjectHandler<ToggleButton> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<ToggleButton>(delegate (ToggleButton component, object data)
        {
            component.OnRefreshUserMenu(data);
        });

#pragma warning disable CS0649
        [MyCmpGet]
        LogicPorts logicPorts;

        [MyCmpReq]
        KSelectable selectable;
#pragma warning restore CS0649

        private Guid statusGuid;

        private delegate void OnLogicValueChangedDelegate(LogicPorts instance, HashedString port_id, int new_value);
        private readonly OnLogicValueChangedDelegate onLogicValueChanged = (OnLogicValueChangedDelegate)typeof(LogicPorts).GetMethod("OnLogicValueChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).CreateDelegate(typeof(OnLogicValueChangedDelegate));

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            if (this.IsEnabled)
                statusGuid = selectable.AddStatusItem(AutomationBypassPatches.bypassStatusItem, this);
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.RefreshUserMenu);

            if (statusGuid != Guid.Empty)
                statusGuid = selectable.RemoveStatusItem(statusGuid);

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

            var port = logicPorts.inputPortInfo[0];
            var portId = port.id;

            if (IsEnabled)
            {
                onLogicValueChanged(logicPorts, portId, 1);
                statusGuid = selectable.AddStatusItem(AutomationBypassPatches.bypassStatusItem, this);
                return;
            }

            statusGuid = selectable.RemoveStatusItem(AutomationBypassPatches.bypassStatusItem);

            LogicCircuitNetwork network = Game.Instance.logicCircuitManager.GetNetworkForCell(logicPorts.inputPorts[0].GetLogicUICell());
            int logicValue = network != null ? (network.IsBitActive(0) ? 1 : 0) : 1;
            onLogicValueChanged(logicPorts, portId, logicValue);
        }

        private void OnRefreshUserMenu(object data)
        {
            bool isEnabled = IsEnabled;
            KIconButtonMenu.ButtonInfo buttonInfo = null;
            buttonInfo = !isEnabled
            ? new KIconButtonMenu.ButtonInfo("action_rocket_restriction_uncontrolled", UI.USERMENUACTIONS.AutomationBypass.NAME, OnMenuToggle, Action.NumActions, null, null, null, "")
            : new KIconButtonMenu.ButtonInfo("action_rocket_restriction_controlled", UI.USERMENUACTIONS.AutomationBypass.NAME_OFF, OnMenuToggle, Action.NumActions, null, null, null, "");
            Game.Instance.userMenu.AddButton(base.gameObject, buttonInfo);
        }
    }
}
