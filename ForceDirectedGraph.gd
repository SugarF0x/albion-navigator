class_name ForceDirectedGraph extends Node2D

const DUMMY_NODE = preload("res://DummyNode.tscn")

@export_group("Initial Node Position")
@export var initial_radius: float = 100.0
@export var initial_angle: float = PI * (3 - sqrt(5))

@export_group("Graph Heat")
@export var alpha := 1.0
@export var alpha_min := 0.0001
@export var alpha_target := 0.0
@export var velocity_decay := 0.6

var alpha_decay := 1.0 - pow(alpha_min, 10)
var nodes: Array[ForceGraphNode] = []
var random := RandomNumberGenerator.new()

func _ready() -> void:
	random.seed = "peepee-poopoo".hash()
	_mock_nodes()
	
	var new_nodes: Array[ForceGraphNode] = []
	for child in get_children(): if child is ForceGraphNode: nodes.append(child)
	initializeNodes()

func _process(delta: float) -> void:
	if alpha < alpha_min: return
	step(delta)

func apply_center_force() -> void:
	pass

func apply_many_body_force() -> void:
	pass

func step(delta: float) -> void:
	alpha += (alpha_target - alpha) * alpha_decay * delta
	
	apply_many_body_force()
	apply_center_force()
	
	for node in nodes: node.update_position(delta)

func _mock_nodes() -> void:
	for n in 10: 
		var node: ForceGraphNode = DUMMY_NODE.instantiate()
		add_child(node)

func initializeNodes() -> void:
	for i in nodes.size():
		var node := nodes[i]
		if node.fixed: continue
		if node.position != Vector2.ZERO: continue
		
		var radius: float = initial_radius * sqrt(0.5 + i)
		var angle: float = i * initial_angle
		node.position = Vector2(radius * cos(angle), radius * sin(angle))
