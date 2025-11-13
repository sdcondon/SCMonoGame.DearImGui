using static ImGuiNET.ImGui;
using static SCMonoGame.DearImGui.Demos.GuiElements.GuiElementHelpers;

namespace SCMonoGame.DearImGui.Demos.GuiElements.Concepts;

class MainMenuBar(bool isVisible = false)
{
    public bool IsVisible = isVisible;

    public void Update()
    {
        if (!IsVisible) return;

        if (BeginMainMenuBar())
        {
            if (BeginMenu("File"))
            {
                ExampleFileMenu();
                EndMenu();
            }
            if (BeginMenu("Edit"))
            {
                if (MenuItem("Undo", "CTRL+Z")) { }
                if (MenuItem("Redo", "CTRL+Y", false, false)) { }
                Separator();
                if (MenuItem("Cut", "CTRL+X")) { }
                if (MenuItem("Copy", "CTRL+C")) { }
                if (MenuItem("Paste", "CTRL+V")) { }
                EndMenu();
            }
            EndMainMenuBar();
        }
    }
}
