using System.Runtime.InteropServices;
using Godot;

namespace AlbionNavigator.Autoload;

public partial class ScreenCapture : Node
{
    [DllImport("user32.dll")]
    private static extern int GetAsyncKeyState(int vKey);
    
    const int VK_CONTROL = 0x11;
    const int VK_S = 0x53;
    
    private bool DidJustPressBind;
    private bool DidReleaseBind = true;

    public override void _Ready()
    {
        
    }
    
    public override void _Process(double delta)
    {
        CheckForBindPress();
        if (DidJustPressBind)
        {
            GD.Print("Control + S pressed globally!");
        }
    }

    private void CheckForBindPress()
    {
        if (!DidReleaseBind) DidReleaseBind = GetAsyncKeyState(VK_CONTROL) == 0 || GetAsyncKeyState(VK_S) == 0;
        if (DidJustPressBind) DidJustPressBind = false;
        if (!DidReleaseBind) return;

        DidJustPressBind = GetAsyncKeyState(VK_CONTROL) > 0 && GetAsyncKeyState(VK_S) > 0;
        if (DidJustPressBind) DidReleaseBind = false;
    }
}