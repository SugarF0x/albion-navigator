using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Godot;

namespace AlbionNavigator.Autoload;

[GlobalClass]
public partial class ScreenCapture : Node
{
    [Signal]
    public delegate void ScreenCapturedEventHandler(Texture2D texture);
    
#if GODOT_WINDOWS
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
        var isControlPressed = GetAsyncKeyState(VK_CONTROL) > 0;
        var isSPressed = GetAsyncKeyState(VK_S) > 0;
        
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

        var width = bitmap.Width;
        var height = bitmap.Height;
        var image = Godot.Image.CreateEmpty(width, height, false, Godot.Image.Format.Rgba8);
            
        var rect = new Rectangle(0, 0, width, height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        var pixelData = new byte[bitmapData.Stride * height];
        Marshal.Copy(bitmapData.Scan0, pixelData, 0, pixelData.Length);
        bitmap.UnlockBits(bitmapData);

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            (pixelData[i], pixelData[i + 2]) = (pixelData[i + 2], pixelData[i]);
        }

        image.SetData(width, height, false, Godot.Image.Format.Rgba8, pixelData);
            
        EmitSignal(SignalName.ScreenCaptured, ImageTexture.CreateFromImage(image));
    }
#endif
}