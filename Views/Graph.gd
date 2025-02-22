extends Node2D

@onready var zome_map: ForceDirectedGraph = $ZoneMap
@onready var map_background: Sprite2D = $MapBackground
@onready var camera_2d: Camera2D = $GraphCamera
@onready var simulation_state_label: Label = %SimulationStateLabel

func _ready() -> void:
	simulation_state_label.visible = false
	center_children()
	zome_map.SimulationStarted.connect(on_simulation_started)
	zome_map.SimulationStopped.connect(on_simulation_stopped)

func center_children() -> void:
	var center := get_viewport_rect().size / 2;
	zome_map.position = center
	map_background.position = center

func on_simulation_started() -> void:
	simulation_state_label.visible = true

func on_simulation_stopped() -> void:
	simulation_state_label.visible = false
