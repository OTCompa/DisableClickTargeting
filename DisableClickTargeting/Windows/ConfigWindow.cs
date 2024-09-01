using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 135);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var enablePlugin = Configuration.EnablePlugin;
        if (ImGui.Checkbox("Enable plugin", ref enablePlugin))
        {
            Configuration.EnablePlugin = enablePlugin;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var lmbOption = Configuration.DisableLMB;
        if (ImGui.Checkbox("Disable click targeting for left click", ref lmbOption))
        {
            Configuration.DisableLMB = lmbOption;
            Configuration.Save();
        }

        var rmbOption = Configuration.DisableRMB;
        if (ImGui.Checkbox("Disable click targeting for right click", ref rmbOption))
        {
            Configuration.DisableRMB = rmbOption;
            Configuration.Save();
        }
    }
}
