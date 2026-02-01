using ImGuiNET;
using System.Runtime.InteropServices;

namespace SCMonoGame.DearImGui;

public static class ImGuiIOPtrExtensions
{
    public static void SetIniFilePath(this ImGuiIOPtr imGuiIoPtr, string iniFilePath)
    {
        unsafe
        {
            // If its set, assume we must have set it with a prior call to this method
            // (we default it to NULL on ImGuiRenderer initialization), so can safely free it like this:
            if (imGuiIoPtr.NativePtr->IniFilename != null)
            {
                Marshal.FreeHGlobal((nint)imGuiIoPtr.NativePtr->IniFilename);
            }

            imGuiIoPtr.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi(iniFilePath);
        }
    }
}
