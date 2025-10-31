using System.Collections.Generic;
using Godot;

namespace AlbionNavigator.Services;

public class LogBox
{
    private static LogBox _instance;
    public static LogBox Instance => _instance ??= new LogBox();
    
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
