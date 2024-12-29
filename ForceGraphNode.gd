class_name ForceGraphNode extends Node2D

@export var fixed := false
@export var strength := -30.0
@export var velocity_decay := 0.6

var index := -1
var velocity := Vector2.ZERO
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
