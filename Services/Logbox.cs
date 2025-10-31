using System.Collections.Generic;
using Godot;

namespace AlbionNavigator.Services;

public class Logbox
{
    private static Logbox _instance;
    public static Logbox Instance => _instance ??= new Logbox();
    
    public delegate void NewEntryAddedHandler(Log log);
    public event NewEntryAddedHandler NewEntryAdded;
    
    public List<Log> Log = [];

    public void Add(string message, LogType type = LogType.Default)
    {
        var log = new Log(message, type);
        Log.Add(log);
        NewEntryAdded?.Invoke(log);
    }
}

public struct Log(string message, LogType type)
{
    public string Message = message;
    public string Timestamp = Time.GetDatetimeStringFromSystem();
    public LogType Type = type;
}

public enum LogType
{
    Default,
}
