using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;

namespace AlbionNavigator.Data;

public struct Zone(int id, Zone.ZoneType type, string displayName, int[] connections = null, Vector2? position = null)
{
    public enum ZoneType
    {
        StartingCity, 
        City, 
        SafeArea, 
        Yellow, 
        Red, 
        Black, 
        Road,
    }
    
    public int Id { get; set; } = id;
    public ZoneType Type { get; set; } = type;
    public string DisplayName { get; set; } = displayName;
    public Vector2 Position { get; set; } = position ?? new Vector2(float.NaN, float.NaN);
    public int[] Connections { get; set; } = connections ?? [];

    public static Zone[] LoadZoneBinaries()
    {
        var zones = new List<Zone>();

        using var stream = File.OpenRead("./Data/zones.bin");
        using var reader = new BinaryReader(stream);
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var id = reader.ReadInt32();

            var displayNameLength = reader.ReadInt32();
            var displayName = Encoding.UTF8.GetString(reader.ReadBytes(displayNameLength));

            var positionLength = reader.ReadInt32();
            Vector2? position = null;
            if (positionLength == 2)
            {
                var posX = reader.ReadDouble();
                var posY = reader.ReadDouble();
                position = new Vector2((float)posX, (float)posY);
            }
            else if (positionLength != 0)
            {
                throw new InvalidDataException("Unexpected position length.");
            }
            
            var type = (ZoneType)reader.ReadInt32();

            var connectionsLength = reader.ReadInt32();
            var connections = new int[connectionsLength];
            for (var i = 0; i < connectionsLength; i++)
            {
                connections[i] = reader.ReadInt32();
            }

            zones.Add(new Zone(id, type, displayName, connections, position));
        }

        return zones.ToArray();
    }
}