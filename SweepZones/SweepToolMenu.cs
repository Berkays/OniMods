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

        public event System.Action<ToolMode> OnSettingChanged;

        public static void CreateInstance()
        {
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

        public ToolMode SelectedKey { get; private set; }

        private GameObject choiceList;
        private GameObject content;
        private readonly IDictionary<ToolMode, MenuOption> options;

        public SweepToolMenu()
        {
            options = new Dictionary<ToolMode, MenuOption>();
            SelectedKey = ToolMode.Set;
        }

        public void ClearMenu()
        {
            HideMenu();
            foreach (var option in options)
                Destroy(option.Value.Checkbox);
            options.Clear();
            SelectedKey = ToolMode.Set;
        }

        public ToolParameterMenu.ToggleState GetState(ToolMode key)
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
                if (option.Checkbox == target)
                {
                    if (option.State == ToolParameterMenu.ToggleState.Off)
                    {
                        // Set to on and all others to off
                        foreach (var disableOption in options.Values)
                            if (disableOption != option)
                                disableOption.State = ToolParameterMenu.ToggleState.Off;
                        option.State = ToolParameterMenu.ToggleState.On;
                        SelectedKey = option.ToolMode;
                        OnSettingChanged?.Invoke(option.ToolMode);
                        OnChange();
                    }
                    break;
                }
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
            var prefab = ToolMenu.Instance.toolParameterMenu.widgetPrefab;
            ClearMenu();

            var widgetPrefab = Util.KInstantiateUI(prefab, choiceList, true);
            PUIElements.SetText(widgetPrefab, "Set Zone");
            var toggle = widgetPrefab.GetComponentInChildren<MultiToggle>();
            if (toggle != null)
            {
                var checkbox = toggle.gameObject;
                var option = new MenuOption(ToolMode.Set, checkbox);

                PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                option.State = ToolParameterMenu.ToggleState.On;
                options.Add(option.ToolMode, option);
                toggle.onClick += () => OnClick(checkbox);
            }

            widgetPrefab = Util.KInstantiateUI(prefab, choiceList, true);
            PUIElements.SetText(widgetPrefab, "Clear Zone");
            toggle = widgetPrefab.GetComponentInChildren<MultiToggle>();
            if (toggle != null)
            {
                var checkbox = toggle.gameObject;
                var option = new MenuOption(ToolMode.Clear, checkbox);

                PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                option.State = ToolParameterMenu.ToggleState.Off;
                options.Add(option.ToolMode, option);
                toggle.onClick += () => OnClick(checkbox);
            }

            // Forbid Items
            if (ModIntegrations.ForbidItemsConfiguration.Enabled == false)
                return;

            widgetPrefab = Util.KInstantiateUI(prefab, choiceList, true);
            PUIElements.SetText(widgetPrefab, "Forbid Zone");
            toggle = widgetPrefab.GetComponentInChildren<MultiToggle>();
            if (toggle != null)
            {
                var checkbox = toggle.gameObject;
                var option = new MenuOption(ToolMode.Forbid, checkbox);

                PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                option.State = ToolParameterMenu.ToggleState.Off;
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

        public void ShowMenu()
        {
            content.SetActive(true);
            OnChange();
        }

        public enum ToolMode
        {
            Set = 0,
            Forbid = 1,
            Clear = 2
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