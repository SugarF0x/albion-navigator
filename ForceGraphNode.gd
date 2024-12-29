# TODO: make this RigitBody2D instead? question mark?
class_name ForceGraphNode extends Node2D

var index := -1
var fixed := false
var velocity := Vector2.ZERO
var strength := -300.0
var velocity_decay := 0.6
var connections: Array[int] = []

func update_position() -> void: 
	if fixed: 
		velocity = Vector2.ZERO
		return
	velocity *= velocity_decay
	position += velocity

func _init(position: Vector2 = Vector2.ZERO) -> void:
	self.position = position

func _to_dict() -> Dictionary:
	return {
		"position": position,
		"fixed": fixed,
		"velocity": velocity
	}
