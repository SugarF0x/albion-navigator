class_name ForceGraphLink extends Node2D

@onready var line: Line2D = $Line2D

@export var desired_distance := 30.0

var source := -1
var target := -1
var bias := 1.0
var strength := 1.0

func _init(source := self.source, target := self.target, strength := self.strength) -> void:
	self.source = source
	self.target = target
	self.strength = strength

func draw_link(nodes: Array[ForceGraphNode]) -> void:
	line.clear_points()
	if source < 0 or source >= nodes.size(): return
	if target < 0 or target >= nodes.size(): return
	line.add_point(nodes[source].position)
	line.add_point(nodes[target].position)
