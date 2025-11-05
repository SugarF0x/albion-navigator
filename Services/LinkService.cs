using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Timer = System.Timers.Timer;

namespace AlbionNavigator.Services;

public class LinkService
{
    private readonly Action<string, LogType> Log;
    private readonly Action PersistLinks;
    
    private static LinkService _instance;
    public static LinkService Instance => _instance ??= new LinkService();

    public event Action<ZoneLink, int> NewLinkAdded;
    public event Action<ZoneLink, int> ExpiredLinkRemoved;
    public event Action<ZoneLink, int, int> LinkExpirationUpdated;
    
    /// permanent last, soon-to-expire first
    public List<ZoneLink> Links = [];

    public LinkService() : this(null, null) {}
    public LinkService(Action persist, Action<string, LogType> log)
    {
        PersistLinks = persist ?? DefaultPersist;
        Log = log ?? LogBox.Instance.Add;
    }

    public void RegisterLinks()
    {
        RegisterStaticLinks();
        LoadStoreLinks();
    }

    private void RegisterStaticLinks()
    {
        var zoneService = ZoneService.Instance;
        if (zoneService == null) throw new FieldAccessException("Cant access zone service when not ready");

        var zones = zoneService.Zones;
        foreach (var zone in zones)
        {
            var source = zone.Id;
            foreach (var target in zone.Connections.Where(connection => connection > source))
            {
                AddLink(source, target, null);
            }
        }
    }
    
    public bool AddLink(int source, int target, string expiration) => AddLink(new ZoneLink(source, target, expiration));
    public bool AddLink(ZoneLink[] links) => links.Aggregate(false, (current, zoneLink) => current | AddLink(zoneLink));
    public bool AddLink(ZoneLink newLink)
    {
        if (newLink.IsPermanent)
        {
            var insertedAt = InsertLink(newLink);
            NewLinkAdded?.Invoke(newLink, insertedAt);
            return true;
        }

        var expirationUpdateFromIndex = -1;
        for (var i = 0; i < Links.Count; i++)
        {
            var link = Links[i];
            if (link.IsPermanent) break;
            if (!link.IsSameSignature(newLink)) continue;
            if (!link.IsLaterThanExpiration(newLink.Expiration)) return false;

            expirationUpdateFromIndex = i;
            Links.RemoveAt(i);
            break;
        }

        var index = InsertLink(newLink);
        if (expirationUpdateFromIndex >= 0) LinkExpirationUpdated?.Invoke(newLink, expirationUpdateFromIndex, index);
        else NewLinkAdded?.Invoke(newLink, index);
        
        PersistLinks();
        return true;
    }
    
    // TODO: performance can be increased by List.BinarySearch
    private int InsertLink(ZoneLink newLink)
    {
        if (Links.Count == 0 || newLink.IsPermanent)
        {
            Links.Add(newLink);
            ScheduleExpiration();
            return Links.Count - 1;
        }

        for (var i = 0; i < Links.Count; i++)
        {
            var link = Links[i];
            if (!link.IsPermanent && !newLink.IsLaterThanExpiration(link.Expiration)) continue;
            
            Links.Insert(i, newLink);
            ScheduleExpiration();
            return i;
        }

        Links.Add(newLink);
        ScheduleExpiration();
        return Links.Count - 1;
    }
    
    #region Expiration
    
    private Timer ExpirationTimer;

    private void ScheduleExpiration()
    {
        ExpirationTimer?.Stop();
        ExpirationTimer?.Dispose();

        if (Links.Count == 0) return;
        var link = Links.First();

        if (link.IsPermanent) return;
        var now = DateTimeOffset.Now;
        if (!DateTimeOffset.TryParse(link.Expiration, out var expiration))
        {
            Log("Failed to schedule expiration", LogType.Error);
            return;
        }

        var delay = (expiration - now).TotalMilliseconds;
        if (delay <= 0)
        {
            PopExpiredLinks();            
            return;
        }

        ExpirationTimer = new Timer(delay) { AutoReset = false };
        ExpirationTimer.Elapsed += (_, _) => PopExpiredLinks();
        ExpirationTimer.Start();
    }
    
    private void PopExpiredLinks()
    {
        var indexesToPop =  new List<int>();
        for (var i = 0; i < Links.Count; i++)
        {
            var link = Links[i];
            if (link.IsPermanent) break;
            if (DateTimeOffset.TryParse(link.Expiration, out var dt) && dt < DateTimeOffset.UtcNow) indexesToPop.Add(i);
        }

        indexesToPop.Reverse();
        foreach (var i in indexesToPop)
        {
            var link = Links[i];
            Links.RemoveAt(i);
            ExpiredLinkRemoved?.Invoke(link, i);
        }

        ScheduleExpiration();
    }
    #endregion
    #region Persistence
    
    private const int Version = 0;
    private const string SavePath = "user://store.save";
    private const string SampleSavePath = "user://sample_store.save";
    
    private void LoadStoreLinks() => LoadLinksFromFile(SavePath);
    public void LoadSampleLinks() => LoadLinksFromFile(SampleSavePath, true);

    private void LoadLinksFromFile(string path, bool overrideTimestamps = false)
    {
        if (!FileAccess.FileExists(path)) return;
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        
        var content = file.GetAsText().Split("|");
        var version = int.Parse(content[0]);
        var data = content[1].Split(";");

        if (version != Version) return;

        foreach (var item in data)
        {
            if (item == "") continue;
            
            try
            {
                var chunks = item.Split(',');
                var source = int.Parse(chunks[0]);
                var target = int.Parse(chunks[1]);
                var expiration = overrideTimestamps ? FiveMinuteOffsetTimestamp : chunks[2];
                if (DateTimeOffset.TryParse(expiration, out var timestamp) && timestamp > DateTimeOffset.UtcNow) AddLink(source, target, expiration);
            }
            catch
            {
                Log("Failed to parse store link: " + item, LogType.Error);
            }
        }
    }
    
    private void DefaultPersist()
    {
        var portalConnections = Links.Where(link => !link.IsPermanent).ToList();
        var dataString = portalConnections.Aggregate($"{Version}|", (current, link) => current + $"{link.Source},{link.Target},{link.Expiration};");
        
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file.StoreString(dataString);
    }

    public void FlushStorage()
    {
        if (!FileAccess.FileExists(SavePath)) return;
        DirAccess.RemoveAbsolute(SavePath);
    }

    private static string FiveMinuteOffsetTimestamp => DateTimeOffset.UtcNow.AddMinutes(5).ToString("O");
    
    #endregion
}

public readonly struct ZoneLink(int source, int target, string expiration) : IEquatable<ZoneLink>
{
    public readonly int Source = source;
    public readonly int Target = target;
    // TODO: store expiration as long and only stringify/parse it during persist/load
    public readonly string Expiration = expiration;

    public bool IsPermanent => Expiration == null;
    
    public bool IsSameSignature(ZoneLink value) => value.Source == Source && value.Target == Target;

    // TODO: perhaps add > and < operators to compare expirations 
    /// <returns>
    /// true if timestamp is later than its own expiration,
    /// false otherwise
    /// </returns>
    public bool IsLaterThanExpiration(string timestamp)
    {
        if (
            DateTimeOffset.TryParse(timestamp, out var value) 
            && DateTimeOffset.TryParse(Expiration, out var expiration)
        ) return value > expiration;
        return false;
    }

    public override string ToString() => $"ZoneLink({Source}, {Target}, {Expiration})";
    
    public static bool operator == (ZoneLink left, ZoneLink right) => left.Equals(right);
    public static bool operator != (ZoneLink left, ZoneLink right) => left.Equals(right);

    public bool Equals(ZoneLink other)
    {
        return Source == other.Source && Target == other.Target && Expiration == other.Expiration;
    }

    public override bool Equals(object obj)
    {
        return obj is ZoneLink other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, Target, Expiration);
    }
}