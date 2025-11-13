using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Resources;
using Godot;

namespace AlbionNavigator.Components.Zone;

public partial class ZoneComponentStack : HFlowContainer
{
	[Export] public PackedScene ZoneComponentScene;

	private ZoneComponent[] _components = [];
	[Export] public ZoneComponent[] Components
	{
		get => _components;
		set
		{
			_components = value;
			SyncView();
		}
	}

	public override void _Ready()
	{
		if (ZoneComponentScene == null) throw new ArgumentNullException(nameof(ZoneComponentScene));
		if (ZoneComponentScene.Instantiate() is not ZoneComponentView) throw new InvalidCastException("ZoneComponentScene is not a ZoneComponentView");

		SyncView();
	}

	private void SyncView()
	{
		if (!IsNodeReady()) return;
		foreach (var child in GetChildren()) child.QueueFree();

		var componentCount = new Dictionary<int, int>();
		foreach (var component in Components.Where(component => component != null)) componentCount[component.Id] = componentCount.GetValueOrDefault(component.Id, 0) + 1;

		foreach (var componentId in componentCount.Keys)
		{
			var child = ZoneComponentScene.Instantiate<ZoneComponentView>();
			child.Count = componentCount[componentId];
			child.Component = Components.First(component => component.Id == componentId);
			AddChild(child);
		}
	}
}