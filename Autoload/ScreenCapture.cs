using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Godot;

namespace AlbionNavigator.Autoload;

[GlobalClass]
public partial class ScreenCapture : Node
{
#if GODOT_WINDOWS
    [DllImport("user32.dll")]
    private static extern int GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    
    const int VK_CONTROL = 0x11;
    const int VK_S = 0x53;
    
    private bool DidJustPressBind;
    private bool DidReleaseBind = true;
    
    [Signal]
    public delegate void ScreenCapturedEventHandler();

    public override void _Ready()
    {
        
    }
    
    public override void _Process(double delta)
    {
        CheckForBindPress();
        if (!DidJustPressBind) return;
        
        TakeScreenshot();
    }

    private void CheckForBindPress()
    {
        if (!DidReleaseBind) DidReleaseBind = GetAsyncKeyState(VK_CONTROL) == 0 || GetAsyncKeyState(VK_S) == 0;
        if (DidJustPressBind) DidJustPressBind = false;
        if (!DidReleaseBind) return;

        DidJustPressBind = GetAsyncKeyState(VK_CONTROL) > 0 && GetAsyncKeyState(VK_S) > 0;
        if (DidJustPressBind) DidReleaseBind = false;
    }

    private void TakeScreenshot()
    {
        var screenWidth = GetSystemMetrics(0);
        var screenHeight = GetSystemMetrics(1);

        using (var bitmap = new System.Drawing.Bitmap(screenWidth, screenHeight))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
            }

            bitmap.Save("D:\\Godot\\_TEMP\\image.png", ImageFormat.Png);
        }
        
        EmitSignal(SignalName.ScreenCaptured);
    }
#endif
}