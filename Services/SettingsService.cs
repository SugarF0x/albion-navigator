using System;
using System.Reflection;
using System.Text.Json;
using AlbionNavigator.Utils;
using Godot;

namespace AlbionNavigator.Services;

public class SettingsService
{
    #region Setup
    
    private static SettingsService _instance;
    public static SettingsService Instance => _instance ??= LoadSettings() ?? CreateInstance();

    /// <summary>
    /// auto subscribe to all property changes tagged with [SubscribeOnInit] and call
    /// debounced persist when they do
    /// </summary>
    private void SubscribeChangeSelf()
    {
        var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (!Attribute.IsDefined(prop, typeof(SubscribeOnInitAttribute))) continue;

            var value = prop.GetValue(this);
            if (value == null) continue;

            var type = prop.PropertyType;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Observable<>)) continue;

            var changedEvent = type.GetEvent("Changed");
            if (changedEvent == null) continue;
            
            changedEvent.AddEventHandler(value, () => PersistDebouncer.Debounce(Persist));
        }
    }

    private const string SavePath = "user://settings.json";
    
    private readonly Debouncer PersistDebouncer = new (500);
    private void Persist()
    {
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(this));
    }

    private static SettingsService LoadSettings()
    {
        if (!FileAccess.FileExists(SavePath)) return null;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        var instance = JsonSerializer.Deserialize<SettingsService>(file.GetAsText());
        instance.SubscribeChangeSelf();
        return instance;
    }

    private static SettingsService CreateInstance()
    {
        var instance = new SettingsService();
        instance.SubscribeChangeSelf();
        return instance;
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class SubscribeOnInitAttribute : Attribute;
    
    #endregion
    #region Properties

    public int Version { get; init; } = 1;
    [SubscribeOnInit] public Observable<float> Volume { get; init; } = new() { Value = 1f };

    #endregion
}
