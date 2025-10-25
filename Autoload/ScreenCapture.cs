using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using AlbionNavigator.Entities;
using AlbionNavigator.Services;
using AlbionNavigator.Utils;
using Godot;

namespace AlbionNavigator.Autoload;

[GlobalClass]
public partial class ScreenCapture : Node
{
    [Signal]
    public delegate void ScreenCapturedEventHandler(Texture2D texture);

    private ZoneMap ZoneMap;
    
    public override void _Ready()
    {
        base._Ready();
        var graph = GetTree().GetFirstNodeInGroup("ForceGraph");
        if (graph is not ZoneMap zoneMap) return;
        ZoneMap = zoneMap;
    }

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
            ProcessImage(bitmap);
        }
        catch (InvalidImage)
        {
            GD.Print("Failed to parse image");
        }
    }

    private void ProcessImage(System.Drawing.Bitmap bitmap)
    {
        var zoneNames = ZoneService.Instance.Zones.Select(zone => zone.DisplayName).ToArray();

        var parseData = MapDataParser.Parse(bitmap);
        var source = FuzzySharp.Process.ExtractOne(parseData.Source, zoneNames);
        var target = FuzzySharp.Process.ExtractOne(parseData.Target, zoneNames);
        var timeout = GetExpiration(parseData.Timeout);
        
        ZoneMap.AddPortal(source.Index, target.Index, timeout);
    }

    private static string GetExpiration(string timestamp)
    {
        var add = TimeSpan.ParseExact(timestamp, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
        var now = DateTime.Now;
        var result = now.Add(add);
        return result.ToString("yyyy-MM-ddTHH:mm:ss");
    }
#endif
}