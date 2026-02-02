using ImGuiNET;

namespace SCDearImGui.MonoGame;

public class FontRegistration
{
    internal FontRegistration(string ttfFilePath, float defaultSizePixels)
    {
        TtfFilePath = ttfFilePath;
        DefaultSizePixels = defaultSizePixels;
    }

    public string TtfFilePath { get; }

    public float DefaultSizePixels { get; }

    public ImFontPtr FontPtr { get; internal set; }
}
