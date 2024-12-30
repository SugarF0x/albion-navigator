extends Node2D

@onready var fps_label: Label = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/FPSLabel
@onready var alpha_label: Label = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/AlphaLabel
@onready var force_directed_graph: ForceDirectedGraph = $ForceDirectedGraph
@onready var button: Button = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/Button

func _ready() -> void:
	button.pressed.connect(force_directed_graph.reheat)

func _process(delta: float) -> void:
	fps_label.text = "FPS: {fps}".format({ "fps": roundf((1 / delta) * 100.0) / 100.0 })
	alpha_label.text = "Alpha: {alpha}".format({ "alpha": force_directed_graph.alpha })
