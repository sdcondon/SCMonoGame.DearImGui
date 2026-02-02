using ImGuiNET;
using static ImGuiNET.ImGui;
using static SCDearImGui.MonoGame.Demos.GuiElements.GuiElementHelpers;

namespace SCDearImGui.MonoGame.Demos.GuiElements.MiniApps;

// Demonstrate creating a window covering the entire screen/viewport
// https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp#L9756
class SimpleFullscreenWindow(bool isOpen = false)
{
    public bool IsOpen = isOpen;

    private bool use_work_area = true;
    private ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings;

    public void Update()
    {
        if (!IsOpen) return;

        // We demonstrate using the full viewport area or the work area (without menu-bars, task-bars etc.)
        // Based on your use case you may want one or the other.
        ImGuiViewportPtr viewport = GetMainViewport();
        SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
        SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);

        if (Begin("Example: Fullscreen window", ref IsOpen, flags))
        {
            Checkbox("Use work area instead of main area", ref use_work_area);
            SameLine();          
            HelpMarker("Main Area = entire viewport,"
                + "\nWork Area = entire viewport minus sections used by the main menu bars, task bars etc."
                + "\n\nEnable the main-menu bar in Examples menu to see the difference.");

            CheckboxFlags(ref flags, ImGuiWindowFlags.NoBackground);
            CheckboxFlags(ref flags, ImGuiWindowFlags.NoDecoration);
            Indent();
            CheckboxFlags(ref flags, ImGuiWindowFlags.NoTitleBar);
            CheckboxFlags(ref flags, ImGuiWindowFlags.NoCollapse);
            CheckboxFlags(ref flags, ImGuiWindowFlags.NoScrollbar);
            Unindent();

            if (Button("Close this window"))
                IsOpen = false;
        }

        End();
    }
}
