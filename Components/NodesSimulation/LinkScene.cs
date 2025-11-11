using System.Linq;
using AlbionNavigator.Resources;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Components.NodesSimulation;

public partial class LinkScene : Line2D
{
	private ZoneLink _link;
	public ZoneLink Link
	{
		get => _link;
		set
		{
			_link = value;
			InitHighlight();
		}
	}

	public override void _Ready()
	{
		NavigationService.Instance.LastInspectedPathUpdated += path =>
			Highlight(IsLinkInPath(path) ? HighlightType.Path : DefaultHighlightType);
	}

	private HighlightType DefaultHighlightType = HighlightType.Default;
	private void InitHighlight()
	{
		var sourceNode = ZoneService.Instance.Zones[Link.Source];
		var targetNode = ZoneService.Instance.Zones[Link.Target];

		Zone.ZoneType[] zoneTypes = [sourceNode.Type, targetNode.Type];
        
		if (zoneTypes.All(type => type == Zone.ZoneType.City)) DefaultHighlightType = HighlightType.CityPortal;
		if (zoneTypes.Any(type => type == Zone.ZoneType.Road) && zoneTypes.Any(type => type != Zone.ZoneType.Road)) DefaultHighlightType = HighlightType.RoadToContinent;
        
		Highlight();
	}

	// TODO: add outline
	
	public void Highlight() => Highlight(DefaultHighlightType);
	public void Highlight(HighlightType type)
	{
		Width = 5f;
		switch (type)
		{
			case HighlightType.Default:
				DefaultColor = Colors.White;
				Width = 2f;
				return;
			case HighlightType.CityPortal: 
				DefaultColor = Colors.Orange;
				Width = 2f;
				return;
			case HighlightType.RoadToContinent: 
				DefaultColor = Colors.Cyan with { A = .5f };
				Width = 2f;
				return;
			case HighlightType.Path: 
				DefaultColor = Colors.Purple;
				Width = 10f;
				return;
			case HighlightType.WayOut: 
				DefaultColor = Colors.Red;
				Width = 10f;
				return;
			default: GD.Print("Unknown highlight type"); return;
		}
	}
	
	public enum HighlightType
	{
		Default,
		CityPortal,
		RoadToContinent,
		Path,
		WayOut,
	}

	private bool IsLinkInPath(int[] path)
	{
		for (var i = 0; i < path.Length - 1; i++) 
			if (
				(path[i] == Link.Source && path[i + 1] == Link.Target)
				|| (path[i] == Link.Target && path[i + 1] == Link.Source)
				) return true;
		return false;
	}
}