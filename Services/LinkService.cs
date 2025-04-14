using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbionNavigator.Autoload.Services;

public struct ZoneLink
{
    public ZoneLink(int source, int target, string expiration)
    {
        Connections = [source, target];
        Array.Sort(Connections);
        _expiration = expiration;
    }

    public readonly int[] Connections;
    private readonly string _expiration;

    private Task _expirationTask;
    public Task ExpirationTask
    {
        get
        {
            if (_expiration == null) return null;
            return _expirationTask ??= Task.Delay(GetExpirationInSeconds(_expiration));
        }
    }
    
    private static int GetExpirationInSeconds(string timestamp)
    {
        var targetDateTime = DateTime.Parse(timestamp);
        var currentDateTime = DateTime.UtcNow;
        var difference = targetDateTime - currentDateTime;
        return (int)difference.TotalSeconds;
    }
}

public class LinkService
{
    private static LinkService _instance;
    public static LinkService Instance {
        set => ArgumentNullException.ThrowIfNull(value);
        get => _instance ??= new LinkService();
    }

    public enum LinkUpdateType
    {
        Unknown,
        StaticRegistered,
        PortalRegistered,
        PortalExpired,
    }
    
    public delegate void LinksUpdatedHandler(LinkUpdateType type, ZoneLink link);
    public event LinksUpdatedHandler LinksUpdated;

    public List<ZoneLink> Links = [];

    private LinkService()
    {
        LoadLinks();
    }

    private void LoadLinks()
    {
        var zoneService = ZoneService.Instance;
        if (!zoneService.IsReady) throw new FieldAccessException("Cant access Zone Service when not ready");
        
        for (var i = 0; i < zoneService.Zones.Length; i++)
        {
            var zone = zoneService.Zones[i];
            foreach (var connection in zone.Connections.Where(index => index > i))
            {
                AddLink(i, connection);
            }
        }
    }

    public void AddLink(int source, int target, string expiration = null)
    {
        var link = new ZoneLink(source, target, expiration);
        Links.Add(link);
        // TODO: schedule removal on expiration

        LinksUpdated?.Invoke(GetUpdateType(), link);
        return;

        LinkUpdateType GetUpdateType()
        {
            return string.IsNullOrEmpty(expiration) ? LinkUpdateType.PortalRegistered : LinkUpdateType.StaticRegistered;
        }
    }
}