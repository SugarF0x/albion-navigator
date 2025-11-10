using System;
using System.Reflection;
using System.Text.Json;
using AlbionNavigator.Utils;
using Godot;
using Expression = System.Linq.Expressions.Expression;

namespace AlbionNavigator.Services;

public class SettingsService
{
    #region Setup
    
    private static SettingsService _instance;
    public static SettingsService Instance => _instance ??= LoadSettings() ?? new SettingsService();

    public SettingsService()
    {
        GD.Print("Subscribing");
        SubscribeChangeSelf();
    }

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
            var type = prop.PropertyType;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Observable<>)) continue;
            
            var genericArg = type.GetGenericArguments()[0];
            var eventInfo = type.GetEvent("Changed");

            var param = Expression.Parameter(genericArg, "_");

            var body = Expression.Call(
                Expression.Constant(PersistDebouncer),
                typeof(Debouncer).GetMethod(nameof(Debouncer.Debounce))!,
                Expression.Constant((Action)Persist)
            );

            var lambda = Expression.Lambda(
                typeof(Action<>).MakeGenericType(genericArg),
                body,
                param
            );

            var handler = lambda.Compile();
            eventInfo?.AddEventHandler(value, handler);
        }
    }

    private const string SavePath = "user://settings.json";
    
    private readonly Debouncer PersistDebouncer = new (500);
    private void DebouncedPersist(object _) => PersistDebouncer.Debounce(Persist); 
    private void Persist()
    {
        GD.Print("PERSISTING");
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(this));
    }

    private static SettingsService LoadSettings()
    {
        if (!FileAccess.FileExists(SavePath)) return null;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        var e = JsonSerializer.Deserialize<SettingsService>(file.GetAsText());
        GD.Print("Deserialized");
        return e;
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class SubscribeOnInitAttribute : Attribute;
    
    #endregion
    #region Properties

    public int Version { get; init; } = 1;
    [SubscribeOnInit] public Observable<float> Volume { get; init; } = new() { Value = 1f };
    
    #endregion
}
