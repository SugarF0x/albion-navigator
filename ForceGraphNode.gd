# TODO: make this RigitBody2D instead? question mark?
class_name ForceGraphNode extends Node2D

@export var fixed := false
@export var mass := 1.0

var velocity := Vector2.ZERO

func apply_force(force: Vector2) -> void: 
	if fixed: return
	velocity += force / mass

func update_position(delta: float) -> void: 
	if fixed: return
	position += velocity * delta

func _init(position: Vector2 = Vector2.ZERO) -> void:
	self.position = position
