class_name ForceGraphLink extends Node2D

@onready var line_2d: Line2D = $Line2D

var source: int
var target: int
var strength := 1.0

func _init(source: int, target: int, strength := self.strength) -> void:
	self.source = source
	self.target = target
	self.strength = strength
