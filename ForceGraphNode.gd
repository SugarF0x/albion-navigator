class_name ForceGraphNode extends Node2D

@export_group("Forces")
@export var fixed := false
@export var strength := -30.0
@export var velocity_decay := 0.6

@export_group("Initial Node Position")
@export var initial_radius: float = 30.0
@export var initial_angle: float = PI * (3 - sqrt(5))

var index := -1
var velocity := Vector2.ZERO
var connections: Array[int] = []

func update_position() -> void: 
	if fixed: 
		velocity = Vector2.ZERO
		return
	
	velocity *= velocity_decay
	position += velocity

func initialize(index: int) -> void:
	initialize_index(index)
	initialize_position(index)
	initialize_connections()

func initialize_index(index: int) -> void:
	if self.index < 0: self.index = index

func initialize_position(index: int) -> void:
	if fixed: return
	if position != Vector2.ZERO: return
	place_node_spirally()

func initialize_connections() -> void:
	connections.clear()

func place_node_spirally(index := self.index, placement_radius := initial_radius, placement_angle := initial_angle) -> void:
	var radius: float = placement_radius * sqrt(0.5 + index)
	var angle: float = index * placement_angle
	position = Vector2(radius * cos(angle), radius * sin(angle))

func _init(position: Vector2 = Vector2.ZERO) -> void:
	self.position = position

func _to_dict() -> Dictionary:
	return {
		"position": position,
		"fixed": fixed,
		"velocity": velocity
	}
