using System;
using System.Collections.Generic;

namespace AlbionNavigator.Services;

public class LogBox
{
    private static LogBox _instance;
    public static LogBox Instance => _instance ??= new LogBox();
    
    public event Action<Log> NewEntryAdded;
    
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
    public readonly string Timestamp = DateTimeOffset.UtcNow.ToString("O");
    public readonly LogType Type = type;

    public string TimeString => DateTimeOffset.Parse(Timestamp).ToString("T");

    public override string ToString() => $"[{TimeString}] {Message}";
}

public enum LogType
{
    Default,
    Error,
    Warning,
}
