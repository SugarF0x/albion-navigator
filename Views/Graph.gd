extends Node2D

@onready var zome_map: ForceDirectedGraph = $ZoneMap
@onready var map_background: Sprite2D = $MapBackground
@onready var camera_2d: Camera2D = $GraphCamera

func _ready() -> void:
	center_children()

func center_children() -> void:
	var center := get_viewport_rect().size / 2;
	zome_map.position = center
	map_background.position = center
