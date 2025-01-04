extends Node2D

@onready var force_directed_graph: ForceDirectedGraph = $ForceDirectedGraph
@onready var map_background: Sprite2D = $MapBackground
@onready var camera_2d: Camera2D = $GraphCamera

func _ready() -> void:
	center_children()

func center_children() -> void:
	var center := get_viewport_rect().size / 2;
	force_directed_graph.position = center
	map_background.position = center
