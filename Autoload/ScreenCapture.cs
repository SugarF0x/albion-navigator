using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using AlbionNavigator.Services;
using AlbionNavigator.Utils;
using Godot;

namespace AlbionNavigator.Autoload;

[GlobalClass]
public partial class ScreenCapture : Node
{
    [DllImport("user32.dll")]
    private static extern int GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    
    const int VK_CONTROL = 0x11;
    const int VK_S = 0x53;
    
    private bool DidJustPressBind;
    private bool DidReleaseBind = true;
    
    public override void _Process(double delta)
    {
        CheckForBindPress();
        if (DidJustPressBind) TakeScreenshot();
    }

    private void CheckForBindPress()
    {
        var isControlPressed = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
        var isSPressed = (GetAsyncKeyState(VK_S) & 0x8000) != 0;
        
        if (!DidReleaseBind) DidReleaseBind = !isControlPressed || !isSPressed;
        if (DidJustPressBind) DidJustPressBind = false;
        if (!DidReleaseBind) return;

        DidJustPressBind = isControlPressed && isSPressed;
        if (DidJustPressBind) DidReleaseBind = false;
    }

    private void TakeScreenshot()
    {
        var screenWidth = GetSystemMetrics(0);
        var screenHeight = GetSystemMetrics(1);

        using var bitmap = new System.Drawing.Bitmap(screenWidth, screenHeight);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
        }

        try {
            var (source, target, timeout) = MapDataParser.Parse(bitmap);
            LinkService.Instance.AddLink(
                ZoneService.Instance.GetProbableZoneIndexFromDisplayName(source),
                ZoneService.Instance.GetProbableZoneIndexFromDisplayName(target),
                (DateTimeOffset.UtcNow + TimeSpan.ParseExact(timeout, "hh\\:mm\\:ss", CultureInfo.InvariantCulture)).ToString("O")
            );
        }
        catch (InvalidImage)
        {
            GD.Print("Failed to parse image");
        }
    }
}