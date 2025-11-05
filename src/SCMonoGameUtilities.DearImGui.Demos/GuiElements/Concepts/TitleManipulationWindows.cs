using ImGuiNET;
using static ImGuiNET.ImGui;

namespace SCMonoGameUtilities.DearImGui.Demos.GuiElements.Concepts;

// Demonstrate the use of "##" and "###" in identifiers to manipulate ID generation.
// This applies to all regular items as well.
// Read FAQ section "How can I have multiple widgets with the same label?" for details.
// https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp#L9792
class TitleManipulationWindows(bool areOpen = false)
{
    public bool AreOpen = areOpen;

    public void Update()
    {
        if (!AreOpen) return;

        var viewport = GetMainViewport();
        var base_pos = viewport.Pos;

        // By default, Windows are uniquely identified by their title.
        // You can use the "##" and "###" markers to manipulate the display/ID.

        // Using "##" to display same title but have unique identifier.
        SetNextWindowPos(new(base_pos.X + 100, base_pos.Y + 100), ImGuiCond.FirstUseEver);
        Begin("Same title as another window##1");
        Text("This is window 1.\nMy title is the same as window 2, but my identifier is unique.");
        End();

        SetNextWindowPos(new(base_pos.X + 100, base_pos.Y + 200), ImGuiCond.FirstUseEver);
        Begin("Same title as another window##2");
        Text("This is window 2.\nMy title is the same as window 1, but my identifier is unique.");
        End();

        // Using "###" to display a changing title but keep a static identifier "AnimatedTitle"
        SetNextWindowPos(new(base_pos.X + 100, base_pos.Y + 300), ImGuiCond.FirstUseEver);
        string name = $"Animated title {"|/-\\"[(int)(GetTime() / 0.25f) & 3]} {GetFrameCount()}###AnimatedTitle";
        Begin(name);
        Text("This window has a changing title.");
        End();
    }
}
