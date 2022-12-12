using System.Collections.Generic;

namespace SweepZones
{
    public sealed class SweepZoneHoverCard : HoverTextConfiguration
    {
        public SweepZoneHoverCard()
        {
            ToolName = SweepZones.UI.STRINGS.TOOL_HOVER_CARD_TITLE;
        }

        public override void UpdateHoverElements(List<KSelectable> hoveredObjects)
        {
            HoverTextScreen screenInstance = HoverTextScreen.Instance;
            HoverTextDrawer drawer = screenInstance.BeginDrawing();
            drawer.BeginShadowBar();

            DrawTitle(screenInstance, drawer);
            drawer.NewLine();

            drawer.DrawIcon(screenInstance.GetSprite("icon_mouse_left"), 20);
            drawer.DrawText("Drag", Styles_Instruction.Standard);
            drawer.AddIndent(8);

            drawer.DrawIcon(screenInstance.GetSprite("icon_mouse_right"), 20);
            drawer.DrawText("Back", Styles_Instruction.Standard);

            if (SweepToolMenu.Instance.GetState(ToolMode.Sweep) == ToolParameterMenu.ToggleState.On)
            {
                drawer.NewLine();
                drawer.DrawText(string.Format("Sweep Priority {0}", ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority().priority_value.ToString()), Styles_Title.Standard);
            }
            else if (SweepToolMenu.Instance.GetState(ToolMode.Mop) == ToolParameterMenu.ToggleState.On)
            {
                drawer.NewLine();
                drawer.DrawText(string.Format("Mop Priority {0}", ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority().priority_value.ToString()), Styles_Title.Standard);
            }

            drawer.EndShadowBar();
            drawer.EndDrawing();
        }
    }
}