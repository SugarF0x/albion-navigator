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

func initialize(graph_index: int) -> void:
	initialize_index(graph_index)
	initialize_position()
	initialize_connections()

func initialize_index(graph_index: int) -> void:
	if index < 0: index = graph_index

func initialize_position() -> void:
	if fixed: return
	if position != Vector2.ZERO: return
	place_node_spirally()

func initialize_connections() -> void:
	connections.clear()

func place_node_spirally(order_index := index, placement_radius := initial_radius, placement_angle := initial_angle) -> void:
	var radius: float = placement_radius * sqrt(0.5 + order_index)
	var angle: float = order_index * placement_angle
	position = Vector2(radius * cos(angle), radius * sin(angle))

func _init(initial_position := Vector2.ZERO) -> void:
	position = initial_position

func _to_dict() -> Dictionary:
	return {
		"position": position,
		"fixed": fixed,
		"velocity": velocity
	}
