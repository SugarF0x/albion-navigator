@tool
class_name ZoneComponentView extends Control

@onready var primary_texture: TextureRect = %PrimaryTexture
@onready var secondary_texture: TextureRect = %SecondaryTexture
@onready var is_big_label: Label = %IsBigLabel
@onready var tier_label: Label = %TierLabel
@onready var count_container: PanelContainer = %CountContainer
@onready var count_label: Label = %CountLabel

@export var count: int = 1 :
	set(value):
		count = value
		register_count()

@export var component: ZoneComponent :
	set(item):
		component = item
		register_properties()

# TODO: perhaps i should eventually rewrite all gdscript items to c# to avoid duplicating shit

enum ZoneComponentType {
	Dungeon,
	Chest,
	Gather,
	MistsCity,
}

enum ZoneComponentProperty {
	Small,
	Big,
	Keeper,
	Heretic,
	Morgana,
	Undead,
	Avalonian,
	Green,
	Blue,
	Gold,
	Ore,
	Stone,
	Wood,
	Fiber,
	Hide,
}

var icons: Dictionary = {
	"CHEST_BLUE" = preload("res://Assets/Images/ZoneComponentIcons/chest-blue.png"),
	"CHEST_GOLD" = preload("res://Assets/Images/ZoneComponentIcons/chest-gold.png"),
	"CHEST_GREEN" = preload("res://Assets/Images/ZoneComponentIcons/chest-green.png"),
	"DUNGEON" = preload("res://Assets/Images/ZoneComponentIcons/dungeon.png"),
	"FIBER" = preload("res://Assets/Images/ZoneComponentIcons/fiber.png"),
	"HIDE" = preload("res://Assets/Images/ZoneComponentIcons/hide.png"),
	"MIST_CITY" = preload("res://Assets/Images/ZoneComponentIcons/mist-city.png"),
	"ORE" = preload("res://Assets/Images/ZoneComponentIcons/ore.png"),
	"STONE" = preload("res://Assets/Images/ZoneComponentIcons/stone.png"),
	"WOOD" = preload("res://Assets/Images/ZoneComponentIcons/wood.png"),
}

func register_count() -> void:
	if not is_node_ready(): await ready
	
	count_container.visible = count > 1
	count_label.text = str(count)

func register_properties() -> void:
	if not is_node_ready(): await ready
	
	tier_label.text = " T{tier}".format({ "tier": component.Tier }) if component.Tier > 0 else ""
	is_big_label.visible = component.Properties.has(ZoneComponentProperty.Big)
	primary_texture.texture = null
	secondary_texture.texture = null
	
	if component.Type == ZoneComponentType.MistsCity: primary_texture.texture = icons["MIST_CITY"]
	elif component.Type == ZoneComponentType.Dungeon: primary_texture.texture = icons["DUNGEON"]
	elif component.Type == ZoneComponentType.Chest:
		if component.Properties.has(ZoneComponentProperty.Green): primary_texture.texture = icons["CHEST_GREEN"]
		elif component.Properties.has(ZoneComponentProperty.Blue): primary_texture.texture = icons["CHEST_BLUE"]
		elif component.Properties.has(ZoneComponentProperty.Gold): primary_texture.texture = icons["CHEST_GOLD"]
	elif component.Type == ZoneComponentType.Gather:
		var texture_stack: Array[Texture] = []
		for property: ZoneComponentProperty in component.Properties:
			match property:
				ZoneComponentProperty.Ore: texture_stack.append(icons["ORE"])
				ZoneComponentProperty.Stone: texture_stack.append(icons["STONE"])
				ZoneComponentProperty.Wood: texture_stack.append(icons["WOOD"])
				ZoneComponentProperty.Fiber: texture_stack.append(icons["FIBER"])
				ZoneComponentProperty.Hide: texture_stack.append(icons["HIDE"])
		if texture_stack.size() > 0: primary_texture.texture = texture_stack[0]
		if texture_stack.size() > 1: secondary_texture.texture = texture_stack[1]
