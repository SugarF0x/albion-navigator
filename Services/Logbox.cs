using System.Collections.Generic;
using Godot;

namespace AlbionNavigator.Services;

public class Logbox
{
    private static Logbox _instance;
    public static Logbox Instance => _instance ??= new Logbox();
    
    public delegate void NewEntryAddedHandler(Log log);
    public event NewEntryAddedHandler NewEntryAdded;
    
    public List<Log> Logs = [];

    public void Add(string message, LogType type = LogType.Default)
    {
        var log = new Log(message, type);
        Logs.Add(log);
        NewEntryAdded?.Invoke(log);
    }
}

public readonly struct Log(string message, LogType type)
{
    public readonly string Message = message;
    public readonly string Timestamp = Time.GetDatetimeStringFromSystem();
    public readonly LogType Type = type;

    public string TimeString => Time.GetTimeStringFromUnixTime(Time.GetUnixTimeFromDatetimeString(Timestamp));

    public override string ToString() => $"[{TimeString}] {Message}";
}

public enum LogType
{
    Default,
}
