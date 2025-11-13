using System;
using System.Collections.Generic;
using Godot;

namespace AlbionNavigator.Components.Zone;

[Tool]
public partial class ZoneComponentView : Control
{
	private TextureRect PrimaryTexture;
	private TextureRect SecondaryTexture;
	private Label IsBigLabel;
	private Label TierLabel;
	private PanelContainer CountContainer;
	private Label CountLabel;

	private int _count;
	[Export] public int Count
	{
		get => _count;
		set
		{
			_count = value;
			RegisterCount();
		}
	}

	private Resources.ZoneComponent _component;
	[Export] public Resources.ZoneComponent Component
	{
		get => _component;
		set
		{
			_component = value;
			RegisterProperties();
		}
	}

	private enum ComponentIconType
	{
		ChestBlue,
		ChestGold,
		ChestGreen,
		Dungeon,
		Fiber,
		Hide,
		MistCity,
		Ore,
		Stone,
		Wood,
	}

	private Dictionary<ComponentIconType, Texture2D> IconTypeToTextureMap = new ();
	private void SetupTextureMap()
	{
		IconTypeToTextureMap.Add(ComponentIconType.ChestBlue, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/chest-blue.png"));
		IconTypeToTextureMap.Add(ComponentIconType.ChestGold, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/chest-gold.png"));
		IconTypeToTextureMap.Add(ComponentIconType.ChestGreen, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/chest-green.png"));
		IconTypeToTextureMap.Add(ComponentIconType.Dungeon, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/dungeon.png"));
		IconTypeToTextureMap.Add(ComponentIconType.Fiber, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/fiber.png"));
		IconTypeToTextureMap.Add(ComponentIconType.Hide, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/hide.png"));
		IconTypeToTextureMap.Add(ComponentIconType.MistCity, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/mist-city.png"));
		IconTypeToTextureMap.Add(ComponentIconType.Ore, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/ore.png"));
		IconTypeToTextureMap.Add(ComponentIconType.Stone, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/stone.png"));
		IconTypeToTextureMap.Add(ComponentIconType.Wood, GD.Load<Texture2D>("res://Assets/Images/ZoneComponentIcons/wood.png"));
	}

	private void RegisterCount()
	{
		if (!IsNodeReady()) return;
		
		CountContainer.Visible = Count > 1;
		CountLabel.Text = Count.ToString();
	}

	private void RegisterProperties()
	{
		if (!IsNodeReady()) return;
		
		PrimaryTexture.Texture = null;
		SecondaryTexture.Texture = null;
		
		TierLabel.Text = (Component?.Tier ?? 0) > 0 ? $" T{Component.Tier}" : "";
		IsBigLabel.Visible = Component?.Properties.Contains(Resources.ZoneComponent.ZoneComponentProperty.Big) ?? false;
		
		if (Component == null) return;
		switch (Component.Type)
		{
			case Resources.ZoneComponent.ZoneComponentType.Dungeon:
				PrimaryTexture.Texture = IconTypeToTextureMap[ComponentIconType.Dungeon];
				break;
			case Resources.ZoneComponent.ZoneComponentType.Chest:
				if (Component.Properties.Contains(Resources.ZoneComponent.ZoneComponentProperty.Green)) PrimaryTexture.Texture = IconTypeToTextureMap[ComponentIconType.ChestGreen];
				else if (Component.Properties.Contains(Resources.ZoneComponent.ZoneComponentProperty.Blue)) PrimaryTexture.Texture = IconTypeToTextureMap[ComponentIconType.ChestBlue];
				else if (Component.Properties.Contains(Resources.ZoneComponent.ZoneComponentProperty.Gold)) PrimaryTexture.Texture = IconTypeToTextureMap[ComponentIconType.ChestGold];
				break;
			case Resources.ZoneComponent.ZoneComponentType.Gather:
				var textureStack = new List<Texture2D>();
				foreach (var property in Component.Properties)
				{
					switch (property)
					{
						case Resources.ZoneComponent.ZoneComponentProperty.Ore: textureStack.Add(IconTypeToTextureMap[ComponentIconType.Ore]); break;
						case Resources.ZoneComponent.ZoneComponentProperty.Stone: textureStack.Add(IconTypeToTextureMap[ComponentIconType.Stone]); break;
						case Resources.ZoneComponent.ZoneComponentProperty.Wood: textureStack.Add(IconTypeToTextureMap[ComponentIconType.Wood]); break;
						case Resources.ZoneComponent.ZoneComponentProperty.Fiber: textureStack.Add(IconTypeToTextureMap[ComponentIconType.Fiber]); break;
						case Resources.ZoneComponent.ZoneComponentProperty.Hide: textureStack.Add(IconTypeToTextureMap[ComponentIconType.Hide]); break;
						case Resources.ZoneComponent.ZoneComponentProperty.Small:
						case Resources.ZoneComponent.ZoneComponentProperty.Big:
						case Resources.ZoneComponent.ZoneComponentProperty.Keeper:
						case Resources.ZoneComponent.ZoneComponentProperty.Heretic:
						case Resources.ZoneComponent.ZoneComponentProperty.Morgana:
						case Resources.ZoneComponent.ZoneComponentProperty.Undead:
						case Resources.ZoneComponent.ZoneComponentProperty.Avalonian:
						case Resources.ZoneComponent.ZoneComponentProperty.Green:
						case Resources.ZoneComponent.ZoneComponentProperty.Blue:
						case Resources.ZoneComponent.ZoneComponentProperty.Gold:
						default: break;
					}
				}

				if (textureStack.Count > 0) PrimaryTexture.Texture = textureStack[0];
				if (textureStack.Count > 1) SecondaryTexture.Texture = textureStack[1];
				
				break;
			case Resources.ZoneComponent.ZoneComponentType.MistsCity:
				PrimaryTexture.Texture = IconTypeToTextureMap[ComponentIconType.MistCity];
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	public override void _Ready()
	{
		PrimaryTexture = GetNode<TextureRect>("%PrimaryTexture");
		SecondaryTexture = GetNode<TextureRect>("%SecondaryTexture");
		IsBigLabel = GetNode<Label>("%IsBigLabel");
		TierLabel = GetNode<Label>("%TierLabel");
		CountContainer = GetNode<PanelContainer>("%CountContainer");
		CountLabel = GetNode<Label>("%CountLabel");
		
		SetupTextureMap();
		RegisterCount();
		RegisterProperties();
	}
}
