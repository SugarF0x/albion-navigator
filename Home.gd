extends Node2D

@onready var fps_label: Label = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer/FPSLabel

func _process(delta: float) -> void:
	fps_label.text = "FPS: {fps}".format({ "fps": roundf((1 / delta) * 100.0) / 100.0 })
