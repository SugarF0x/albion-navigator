# TODO: make this RigitBody2D instead? question mark?
class_name ForceGraphNode extends Node2D

var fixed := false
var velocity := Vector2.ZERO
var strength := -30.0
var velocity_decay := 0.4

func apply_force(force: Vector2) -> void: 
	if fixed: return
	velocity += force

func update_position(delta: float) -> void: 
	if fixed: return
	position += velocity * delta
	velocity *= 1.0 - velocity_decay * delta

func _init(position: Vector2 = Vector2.ZERO) -> void:
	self.position = position

func _to_dict() -> Dictionary:
	return {
		"position": position,
		"fixed": fixed,
		"velocity": velocity
	}
