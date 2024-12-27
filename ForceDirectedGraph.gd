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

@export_group("Many body aproximation")
@export var theta_squared := 0.81
@export var distance_min_squared := 1.0
@export var distance_max_squared := INF

var alpha_decay := 1.0 - pow(alpha_min, 10)
var nodes: Array[ForceGraphNode] = []
var random := RandomNumberGenerator.new()
var strengths: Array[float] = []

func _ready() -> void:	
	random.seed = "peepee-poopoo".hash()
	_mock_nodes()
	
	for child in get_children(): if child is ForceGraphNode: nodes.append(child)
	initialize_nodes()

func _process(delta: float) -> void:
	if alpha < alpha_min: return
	step(delta)

func apply_center_force(delta: float) -> void:
	var strength := 1.0
	var center := Vector2.ZERO
	var shift := Vector2.ZERO
	
	for node in nodes:
		shift += node.position
	
	shift = shift / nodes.size() - center * strength
	for node in nodes: 
		node.position -= shift * delta

func apply_many_body_force(delta: float) -> void:
	var tree := QuadTree.new().add_all(nodes).visit_after(accumulate)
	for node in nodes: 
		tree.visit(apply.bind(node, delta))

func step(delta: float) -> void:
	alpha += (alpha_target - alpha) * alpha_decay * delta
	
	apply_many_body_force(delta)
	apply_center_force(delta)
	
	for node in nodes: node.update_position(delta)

func initialize_nodes() -> void:
	for i in nodes.size():
		var node := nodes[i]
		if node.fixed: continue
		if node.position != Vector2.ZERO: continue
		
		var radius: float = initial_radius * sqrt(0.5 + i)
		var angle: float = i * initial_angle
		node.position = Vector2(radius * cos(angle), radius * sin(angle))

func accumulate(node: QuadTreeNode, _rect: Rect2) -> void:
	var strength := 0.0
	var weight := 0.0
	var charge := 0.0
	
	if node.is_branch():
		var center_of_mass := Vector2.ZERO
		for branch in node.branches:
			charge = abs(branch.charge)
			if charge == 0.0: continue
			
			strength += branch.charge
			weight += charge
			center_of_mass += center_of_mass * charge
			
		node.center_of_mass = center_of_mass / weight
	else:
		var leaf_position := node.leaves[0].position
		node.center_of_mass = leaf_position
		for leaf in node.leaves: strength += leaf.strength
	
	node.charge = strength

func apply(tree_node: QuadTreeNode, quad_rect: Rect2, graph_node: ForceGraphNode, delta: float) -> bool:
	if tree_node.charge == 0.0: return true
	
	var attraction_direction := tree_node.center_of_mass - graph_node.position
	var quad_width := quad_rect.size.x
	var distance_to_center_of_mass_squared := graph_node.position.distance_squared_to(tree_node.center_of_mass)
	
	if quad_width * quad_width / theta_squared < distance_to_center_of_mass_squared:
		if distance_to_center_of_mass_squared < distance_max_squared:
			if attraction_direction.x == 0.0: attraction_direction.x = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.x, 2)
			if attraction_direction.y == 0.0: attraction_direction.y = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.y, 2)
			if distance_to_center_of_mass_squared < distance_min_squared: distance_to_center_of_mass_squared = sqrt(distance_min_squared * distance_to_center_of_mass_squared)
			graph_node.velocity += attraction_direction * tree_node.charge * alpha / distance_to_center_of_mass_squared * delta
		return true
	
	if tree_node.is_branch() or distance_to_center_of_mass_squared >= distance_max_squared: return false
	
	if tree_node.leaves[0] != graph_node or tree_node.leaves.size() > 1:
		if attraction_direction.x == 0.0: attraction_direction.x = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.x, 2)
		if attraction_direction.y == 0.0: attraction_direction.y = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.y, 2)
		if distance_to_center_of_mass_squared < distance_min_squared: distance_to_center_of_mass_squared = sqrt(distance_min_squared * distance_to_center_of_mass_squared)
	
	for leaf in tree_node.leaves:
		if leaf == graph_node: continue
		graph_node.velocity += attraction_direction * quad_width * delta
	
	return false

func _mock_nodes() -> void:
	for n in 10: 
		var node: ForceGraphNode = DUMMY_NODE.instantiate()
		add_child(node)

func jiggle() -> float:
	return (random.randf() - 0.5) * 1e-6
