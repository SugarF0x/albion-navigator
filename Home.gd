extends Node2D

@onready var fps_label: Label = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/FPSLabel
@onready var alpha_label: Label = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/AlphaLabel
@onready var force_directed_graph: ForceDirectedGraph = $ForceDirectedGraph
@onready var button: Button = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/Button
@onready var map_background: Sprite2D = $"MapBackground"
@onready var camera_2d: Camera2D = $Camera2D

func _ready() -> void:
	button.pressed.connect(force_directed_graph.Reheat)

func _process(delta: float) -> void:
	center_children()
	fps_label.text = "FPS: {fps}".format({ "fps": roundf((1 / delta) * 100.0) / 100.0 })
	alpha_label.text = "Alpha: {alpha}".format({ "alpha": force_directed_graph.Alpha })

func center_children() -> void:
	var center := get_viewport_rect().size / 2;
	force_directed_graph.position = center
	map_background.position = center
