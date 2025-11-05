using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using static ImGuiNET.ImGui;
using static SCMonoGameUtilities.DearImGui.Demos.GuiElements.GuiElementHelpers;

namespace SCMonoGameUtilities.DearImGui.Demos.GuiElements.DemoWindow;

class DemoWindow(Game owner)
{
    // This window is rather large - large enough that having separate
    // types for most of the sections is worthwhile for maintainability:
    private readonly DemoWindowWidgetsSection widgetsSection = new();
    private readonly DemoWindowLayoutAndScrollingSection layoutAndScrollingSection = new();
    private readonly DemoWindowPopupsAndModalsSection popupsAndModalsSection = new();
    private readonly DemoWindowTablesSection tablesSection = new();
    private readonly DemoWindowInputsNavAndFocusSection inputsNavAndFocusSection = new();

    private ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar;

    public List<MenuSection> ExamplesMenuSections { get; } = [];

    public List<MenuSection> ToolsMenuSections { get; } = [];

    public void Update()
    {
        SetNextWindowPos(new(450, 20), ImGuiCond.FirstUseEver);
        SetNextWindowSize(new(550, 680), ImGuiCond.FirstUseEver);

        var exitApp = false;

        if (Begin("Dear ImGui Demo / Monogame & ImGui.Net", windowFlags))
        {
            PushItemWidth(GetFontSize() * -12);

            UpdateMenuBar(ref exitApp);
            UpdateHelpSection();
            UpdateConfigurationSection();
            UpdateWindowOptionsSection();
            widgetsSection.Update();
            layoutAndScrollingSection.Update();
            popupsAndModalsSection.Update();
            tablesSection.Update();
            inputsNavAndFocusSection.Update();

            PopItemWidth();
        }

        End();

        if (exitApp)
        {
            owner.Exit();
        }
    }

    private void UpdateMenuBar(ref bool exitApp)
    {
        if (BeginMenuBar())
        {
            if (BeginMenu("File"))
            {
                MenuItem("Exit", null, ref exitApp);
                EndMenu();
            }

            if (BeginMenu("Examples"))
            {
                UpdateMenuItems(ExamplesMenuSections);
                EndMenu();
            }

            if (BeginMenu("Tools"))
            {
                UpdateMenuItems(ToolsMenuSections);
                EndMenu();
            }

            EndMenuBar();
        }

        static void UpdateMenuItems(List<MenuSection> menuSections)
        {
            foreach (var menuSection in menuSections)
            {
                SeparatorText(menuSection.Title);

                foreach (var menuItem in menuSection.Items)
                {
                    var isSelected = menuItem.GetIsSelected();
                    if (ImGui.MenuItem(menuItem.Text, null, ref isSelected))
                    {
                        menuItem.OnSetIsSelected(isSelected);
                    }
                }
            }
        }
    }

    private static void UpdateHelpSection()
    {
        if (!CollapsingHeader("Help")) return;

        Text("dear imgui says hello. (" + GetVersion() + ")");

        Separator();
        Text("ABOUT THIS DEMO:");
        BulletText("Sections below are demonstrating many aspects of the library.");
        BulletText("The \"Examples\" menu above leads to more demo contents.");
        BulletText("The \"Tools\" menu above gives access to: About Box, Style Editor,\nand Metrics/Debugger (general purpose Dear ImGui debugging tool).");

        Separator();
        Text("PROGRAMMER GUIDE:");
        BulletText("See the ShowDemoWindow() code in imgui_demo.cpp. <- you are here!");
        BulletText("See comments in imgui.cpp.");
        BulletText("See example applications in the examples/ folder.");
        BulletText("Read the FAQ at http://www.dearimgui.org/faq/");
        BulletText("Set 'io.ConfigFlags |= NavEnableKeyboard' for keyboard controls.");
        BulletText("Set 'io.ConfigFlags |= NavEnableGamepad' for gamepad controls.");

        Separator();
        Text("USER GUIDE:");
        ShowUserGuide();
    }

    private static void UpdateConfigurationSection()
    {
        if (!CollapsingHeader("Configuration")) return;

        if (TreeNode("Style"))
        {
            HelpMarker("The same contents can be accessed in 'Tools->Style Editor' or by calling the ShowStyleEditor() function.");
            ShowStyleEditor();
            TreePop();
        }

        if (TreeNode("Capture/Logging"))
        {
            HelpMarker("The logging API redirects all text output so you can easily capture the content of a window or a block. Tree nodes can be automatically expanded.\nTry opening any of the contents below in this window and then click one of the \"Log To\" button.");
            LogButtons();

            HelpMarker("You can also call ImGui.LogText() to output directly to the log without a visual output.");
            if (Button("Copy \"Hello, world!\" to clipboard"))
            {
                LogToClipboard();
                LogText("Hello, world!");
                LogFinish();
            }

            TreePop();
        }
    }

    private void UpdateWindowOptionsSection()
    {
        if (!CollapsingHeader("Window options")) return;

        CheckboxFlags("No title bar", ref windowFlags, ImGuiWindowFlags.NoTitleBar);
        CheckboxFlags("No scrollbar", ref windowFlags, ImGuiWindowFlags.NoScrollbar);
        CheckboxFlags("Menu bar", ref windowFlags, ImGuiWindowFlags.MenuBar);
        CheckboxFlags("No move", ref windowFlags, ImGuiWindowFlags.NoMove);
        CheckboxFlags("No resize", ref windowFlags, ImGuiWindowFlags.NoResize);
        CheckboxFlags("No collapse", ref windowFlags, ImGuiWindowFlags.NoCollapse);
        CheckboxFlags("No nav", ref windowFlags, ImGuiWindowFlags.NoNav);
        CheckboxFlags("No background", ref windowFlags, ImGuiWindowFlags.NoBackground);
        CheckboxFlags("No bring to front on focus", ref windowFlags, ImGuiWindowFlags.NoBringToFrontOnFocus);
    }

    public record MenuSection(string Title) : IEnumerable
    {
        public List<MenuItem> Items { get; } = [];

        public IEnumerator GetEnumerator() => Items.GetEnumerator();

        public void Add(MenuItem item) => Items.Add(item);
    }

    public class MenuItem
    {
        public MenuItem(string text, Expression<Func<bool>> isSelected)
        {
            Text = text;
            GetIsSelected = isSelected.Compile();

            if (isSelected.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("isSelected must be boolean field/property access", nameof(isSelected));
            }

            var assignmentParameter = Expression.Parameter(typeof(bool));
            var assignmentExpression = Expression.Assign(memberExpression, assignmentParameter);
            OnSetIsSelected = Expression.Lambda<Action<bool>>(assignmentExpression, assignmentParameter).Compile();
        }

        public string Text { get; }
        public Func<bool> GetIsSelected { get; }
        public Action<bool> OnSetIsSelected { get; }
    }
}
