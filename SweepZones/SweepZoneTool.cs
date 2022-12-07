using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SweepZones
{
    public class SweepZoneTool : DragTool
    {
        private SweepToolMenu.ToolMode selectedMode = SweepToolMenu.ToolMode.Set;

        private SpriteRenderer toolRenderer;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            interceptNumberKeysForPriority = true;

            visualizer = new GameObject("SweepZoneVisualizer");
            visualizer.SetActive(false);

            GameObject offsetObject = new GameObject();
            SpriteRenderer spriteRenderer = offsetObject.AddComponent<SpriteRenderer>();
            toolRenderer = spriteRenderer;
            spriteRenderer.color = CommonProps.TOOL_COLOR;
            spriteRenderer.sprite = SweepZones.ICONS.SET_VISUALIZER_SPRITE;

            offsetObject.transform.SetParent(visualizer.transform);
            offsetObject.transform.localPosition = new Vector3(0, Grid.HalfCellSizeInMeters);
            var sprite = spriteRenderer.sprite;
            offsetObject.transform.localScale = new Vector3(
                Grid.CellSizeInMeters / (sprite.texture.width / sprite.pixelsPerUnit),
                Grid.CellSizeInMeters / (sprite.texture.height / sprite.pixelsPerUnit)
            );

            offsetObject.SetLayerRecursively(LayerMask.NameToLayer("Overlay"));
            visualizer.transform.SetParent(transform);

            FieldInfo areaVisualizerField = AccessTools.Field(typeof(DragTool), "areaVisualizer");
            FieldInfo areaVisualizerSpriteRendererField = AccessTools.Field(typeof(DragTool), "areaVisualizerSpriteRenderer");

            GameObject aV = Util.KInstantiate((GameObject)AccessTools.Field(typeof(DeconstructTool), "areaVisualizer").GetValue(DeconstructTool.Instance));
            aV.SetActive(false);

            aV.name = "SweepZoneAreaVisualizer";
            SpriteRenderer aVSpriteRenderer = aV.GetComponent<SpriteRenderer>();
            areaVisualizerSpriteRendererField.SetValue(this, aVSpriteRenderer);
            aV.transform.SetParent(transform);
            aVSpriteRenderer.color = CommonProps.TOOL_COLOR;
            aVSpriteRenderer.material.color = CommonProps.TOOL_COLOR;

            areaVisualizerField.SetValue(this, aV);

            gameObject.AddComponent<SweepZoneHoverCard>();
        }

        protected override void OnActivateTool()
        {
            base.OnActivateTool();
            ToolMenu.Instance.PriorityScreen.Show();
            SetMode(Mode.Box);

            // Activate overlay
            OverlayScreen.Instance.ToggleOverlay(newMode: SweepZoneOverlay.ID);

            var menu = SweepToolMenu.Instance;
            if (!menu.HasOptions)
                menu.PopulateMenu();

            menu.ShowMenu();
            menu.OnSettingChanged += OnToolSettingChange;
        }

        protected override void OnDeactivateTool(InterfaceTool new_tool)
        {
            base.OnDeactivateTool(new_tool);
            ToolMenu.Instance.PriorityScreen.Show(false);

            // Deactivate overlay
            OverlayScreen.Instance.ToggleOverlay(newMode: OverlayModes.None.ID);

            var menu = SweepToolMenu.Instance;
            menu.OnSettingChanged -= OnToolSettingChange;
            menu.HideMenu();
        }

        private void OnToolSettingChange(SweepToolMenu.ToolMode toolMode)
        {
            selectedMode = toolMode;

            switch (selectedMode)
            {
                case SweepToolMenu.ToolMode.Set:
                    toolRenderer.sprite = ICONS.SET_VISUALIZER_SPRITE;
                    ToolMenu.Instance.PriorityScreen.Show(true);
                    break;
                case SweepToolMenu.ToolMode.Clear:
                    toolRenderer.sprite = ICONS.CANCEL_VISUALIZER_SPRITE;
                    ToolMenu.Instance.PriorityScreen.Show(false);
                    break;
                case SweepToolMenu.ToolMode.Forbid:
                    toolRenderer.sprite = ICONS.SET_VISUALIZER_SPRITE;
                    ToolMenu.Instance.PriorityScreen.Show(false);
                    break;
            }
        }
        protected override void OnDragComplete(Vector3 cursorDown, Vector3 cursorUp)
        {
            base.OnDragComplete(cursorDown, cursorUp);

            if (hasFocus)
            {
                Grid.PosToXY(cursorDown, out int x0, out int y0);
                Grid.PosToXY(cursorUp, out int x1, out int y1);

                if (x0 > x1)
                {
                    Util.Swap(ref x0, ref x1);
                }

                if (y0 > y1)
                {
                    Util.Swap(ref y0, ref y1);
                }

                var priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();

                for (int x = x0; x <= x1; ++x)
                {
                    for (int y = y0; y <= y1; ++y)
                    {
                        int cell = Grid.XYToCell(x, y);

                        if (Grid.IsValidCell(cell) && Grid.IsVisible(cell) && Grid.Element[cell].id != SimHashes.Unobtanium)
                        {
                            switch (selectedMode)
                            {
                                case SweepToolMenu.ToolMode.Clear:
                                    SaveState.Instance.DeleteCell(cell);
                                    break;
                                case SweepToolMenu.ToolMode.Set:
                                    SaveState.Instance[cell] = priority;
                                    break;
                                case SweepToolMenu.ToolMode.Forbid:
                                    SaveState.Instance[cell] = new PrioritySetting(PriorityScreen.PriorityClass.basic, 10);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}