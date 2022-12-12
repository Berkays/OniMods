using System;
using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace SweepZones
{
    // Reference: https://github.com/peterhaneve/ONIMods/blob/main/BulkSettingsChange/BulkParameterMenu.cs
    public sealed class SweepToolMenu : KMonoBehaviour
    {
        public static SweepToolMenu Instance { get; private set; }

        internal event System.Action<ToolMode> OnSettingChanged;

        public static void CreateInstance()
        {
            DestroyInstance();

            var parameterMenu = new GameObject("SweepZoneSettingsChangeParams");
            var originalMenu = ToolMenu.Instance.toolParameterMenu;
            if (originalMenu != null)
                parameterMenu.transform.SetParent(originalMenu.transform.parent);

            Instance = parameterMenu.AddComponent<SweepToolMenu>();
            parameterMenu.SetActive(true);
            parameterMenu.SetActive(false);
        }

        public static void DestroyInstance()
        {
            var inst = Instance;
            if (inst != null)
            {
                inst.ClearMenu();
                Destroy(inst.gameObject);
            }
            Instance = null;
        }

        public bool HasOptions
        {
            get
            {
                return options.Count > 0;
            }
        }

        internal ToolMode SelectedKey { get; private set; }

        private GameObject choiceList;
        private GameObject content;
        private readonly IDictionary<ToolMode, MenuOption> options;

        public SweepToolMenu()
        {
            options = new Dictionary<ToolMode, MenuOption>();
            SelectedKey = ToolMode.Sweep;
        }

        public void ClearMenu()
        {
            HideMenu();
            foreach (var option in options)
                Destroy(option.Value.Checkbox);
            options.Clear();
            SelectedKey = ToolMode.Sweep;
        }

        internal ToolParameterMenu.ToggleState GetState(ToolMode key)
        {
            var state = ToolParameterMenu.ToggleState.Off;
            if (options.TryGetValue(key, out MenuOption option))
                state = option.State;
            return state;
        }

        public void HideMenu()
        {
            content?.SetActive(false);
        }

        private void OnChange()
        {
            foreach (var option in options.Values)
            {
                var checkbox = option.Checkbox;
                switch (option.State)
                {
                    case ToolParameterMenu.ToggleState.On:
                        PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_CHECKED);
                        break;
                    case ToolParameterMenu.ToggleState.Off:
                        PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_UNCHECKED);
                        break;
                    case ToolParameterMenu.ToggleState.Disabled:
                    default:
                        PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                        break;
                }
            }
        }

        private void OnClick(GameObject target)
        {
            foreach (var option in options.Values)
            {
                if (option.Checkbox == target)
                {
                    if (option.State == ToolParameterMenu.ToggleState.Off)
                    {
                        // Set to on and all others to off
                        foreach (var disableOption in options.Values)
                        {
                            if (disableOption != option)
                                disableOption.State = ToolParameterMenu.ToggleState.Off;
                        }
                        option.State = ToolParameterMenu.ToggleState.On;
                        SelectedKey = option.ToolMode;
                        OnChange();
                    }
                    break;
                }
            }

            OnSettingChanged?.Invoke(SelectedKey);
        }

        protected override void OnCleanUp()
        {
            ClearMenu();
            if (content != null)
                Destroy(content);

            base.OnCleanUp();
        }

        protected override void OnPrefabInit()
        {
            var menu = ToolMenu.Instance.toolParameterMenu;
            var baseContent = menu.content;
            base.OnPrefabInit();
            content = Util.KInstantiateUI(baseContent, baseContent.GetParent(), false);
            var transform = content.rectTransform();
            // Add buttons to the chooser
            if (transform.childCount > 1)
                choiceList = transform.GetChild(1).gameObject;
            // Bump up the offset max to allow more space
            transform.offsetMax = new Vector2(0.0f, 300.0f);
            transform.SetAsFirstSibling();
            HideMenu();
        }

        internal void PopulateMenu()
        {
            ClearMenu();

            createOption("Sweep Zone", ToolMode.Sweep, ToolParameterMenu.ToggleState.On);
            createOption(ModIntegrations.ForbidItemsConfiguration.Enabled ? "Clear Sweep/Forbid" : "Clear Sweep Zone", ToolMode.SweepClear);
            createOption("Mop Zone", ToolMode.Mop);
            createOption("Clear Mop Zone", ToolMode.MopClear);

            // Forbid Items
            if (ModIntegrations.ForbidItemsConfiguration.Enabled == false)
                return;

            createOption("Forbid Zone", ToolMode.Forbid);
        }

        private void createOption(string text, ToolMode mode, ToolParameterMenu.ToggleState state = ToolParameterMenu.ToggleState.Off)
        {
            GameObject originalObj = ToolMenu.Instance.toolParameterMenu.widgetPrefab;

            GameObject widgetObj = Util.KInstantiateUI(originalObj, choiceList, true);
            PUIElements.SetText(widgetObj, text);
            MultiToggle toggle = widgetObj.GetComponentInChildren<MultiToggle>();
            if (toggle != null)
            {
                var checkbox = toggle.gameObject;
                var option = new MenuOption(mode, checkbox);

                PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                option.State = state;
                options.Add(option.ToolMode, option);
                toggle.onClick += () => OnClick(checkbox);
            }
        }

        public void SetAll(ToolParameterMenu.ToggleState toggleState)
        {
            foreach (var option in options)
                option.Value.State = toggleState;
            OnChange();
        }

        internal void SetOption(ToolMode toolMode)
        {
            foreach (MenuOption option in options.Values)
            {
                if (option.ToolMode == toolMode)
                {
                    option.State = ToolParameterMenu.ToggleState.On;
                    SelectedKey = option.ToolMode;
                    continue;
                }
                option.State = ToolParameterMenu.ToggleState.Off;
            }

            OnChange();
            OnSettingChanged?.Invoke(SelectedKey);
        }

        public void ShowMenu()
        {
            content.SetActive(true);
            OnChange();
        }

        private sealed class MenuOption
        {
            public GameObject Checkbox { get; }

            public ToolParameterMenu.ToggleState State { get; set; }

            public ToolMode ToolMode { get; set; }

            public MenuOption(ToolMode tool, GameObject checkbox)
            {
                Checkbox = checkbox ?? throw new ArgumentNullException(nameof(checkbox));
                ToolMode = tool;
                State = ToolParameterMenu.ToggleState.Off;
            }

            public override string ToString()
            {
                return ToolMode.ToString();
            }
        }
    }
}