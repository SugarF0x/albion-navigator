class_name ForceDirectedGraph extends Node2D

@onready var links_container: Node2D = %LinksContainer
@onready var nodes_container: Node2D = %NodesContainer

@export_group("Graph Heat")
@export var alpha := 1.0
@export var alpha_min := 0.0001
@export var alpha_target := 0.0
@export var alpha_decay := 1.0 - pow(alpha_min, 1.0 / 300.0)
@export var alpha_reheat := 0.5
@export var reheat_on_nodes_added := true

@export_group("Many body aproximation")
@export var theta_squared := 0.81
@export var distance_min_squared := 1.0
@export var distance_max_squared := INF

@export_group("Gravitational pull")
@export var gravity_strength := 0.1
@export var central_strength := 1.0

@export_group("Debug")
@export var node_scene: PackedScene = preload("res://DummyNode.tscn")
@export var link_scene: PackedScene = preload("res://DummyLink.tscn")
@export var draw_quad_tree := false
@export var draw_center_of_mass := false
@export var mock_nodes_count := 0
@export var mock_fixed_nodes_count := 0
@export var mock_link_count := 0

var _random := RandomNumberGenerator.new()
var nodes: Array[ForceGraphNode] = []
var links: Array[ForceGraphLink] = []

func _ready() -> void:
	_random.seed = "peepee-poopoo".hash()
	_connect_child_listeners()
	_mock()

func _process(_delta: float) -> void:
	_center_window()
	if alpha >= alpha_min: _step()

func _step() -> void:
	_register_children()
	alpha += (alpha_target - alpha) * alpha_decay
	
	apply_center_force()
	apply_link_force()
	apply_many_body_force()
	apply_gravity_force()
	
	for node in nodes: 
		node.update_position()
	
	for link in links:
		link.draw_link(nodes)

func _center_window() -> void:
	var rect := get_viewport().get_visible_rect()
	position = rect.size / 2

#region Controls

func add_node(node: ForceGraphNode) -> void: nodes_container.add_child(node)
func add_link(link: ForceGraphLink) -> void: links_container.add_child(link)

func reheat(value := alpha_reheat) -> void: alpha = maxf(alpha, value)

#endregion
#region Node registration and initalisation

var _should_register_children := true
func _on_children_changed(_child: Node) -> void: 
	_should_register_children = true
	if reheat_on_nodes_added: reheat()

func _connect_child_listeners() -> void:
	nodes_container.child_entered_tree.connect(_on_children_changed)
	nodes_container.child_exiting_tree.connect(_on_children_changed)
	links_container.child_entered_tree.connect(_on_children_changed)
	links_container.child_exiting_tree.connect(_on_children_changed)

func _register_children() -> void:
	if not _should_register_children: return
	_should_register_children = false
	
	nodes.clear()
	links.clear()
	
	for child in nodes_container.get_children():
		if child is ForceGraphNode: nodes.append(child)
	
	for child in links_container.get_children():
		if child is ForceGraphLink: links.append(child)
	
	initialize_entities()

func initialize_entities() -> void:
	for index in nodes.size():
		nodes[index].initialize(index)
	
	for index in links.size():
		links[index].initialize(nodes)

#endregion
#region Link force

func apply_link_force() -> void:
	for link in links:
		var source_node := nodes[link.source]
		var target_node := nodes[link.target]
		
		var spring_velocity := target_node.position + target_node.velocity - source_node.position - source_node.velocity
		if spring_velocity.x == 0.0: spring_velocity.x = jiggle()
		if spring_velocity.y == 0.0: spring_velocity.y = jiggle()
		
		var length := target_node.position.distance_to(source_node.position)
		var adjusted_length_multiplier := (length - link.desired_distance) / length * alpha * link.strength
		spring_velocity *= adjusted_length_multiplier
		
		target_node.velocity -= spring_velocity * link.bias
		source_node.velocity += spring_velocity * (1.0 - link.bias)

#endregion
#region Gravity force

func apply_gravity_force() -> void:
	for node in nodes:
		node.velocity -= node.position * gravity_strength * alpha

#endregion
#region Central force

func apply_center_force() -> void:
	var center := Vector2.ZERO
	var shift := Vector2.ZERO
	
	for node in nodes:
		shift += node.position
	
	shift = shift / nodes.size() - center * central_strength
	for node in nodes:
		if node.fixed: continue 
		node.position -= shift

#endregion
#region Many body force

var tree := QuadTree.new()

func apply_many_body_force() -> void:
	tree = QuadTree.new().add_all(nodes).visit_after(accumulate)
	#for node in nodes: 
		#tree.visit(apply.bind(node))
	
	queue_redraw()

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
			center_of_mass += branch.center_of_mass * charge
			
		node.center_of_mass = center_of_mass / weight
	else:
		var leaf_position := node.leaves[0].position
		node.center_of_mass = leaf_position
		for leaf in node.leaves: strength += leaf.strength
	
	node.charge = strength

func apply(tree_node: QuadTreeNode, quad_rect: Rect2, graph_node: ForceGraphNode) -> bool:
	if tree_node.charge == 0.0: return true
	
	var attraction_direction := tree_node.center_of_mass - graph_node.position
	var quad_width := quad_rect.size.x
	var distance_to_center_of_mass_squared := graph_node.position.distance_squared_to(tree_node.center_of_mass)
	
	if quad_width * quad_width / theta_squared < distance_to_center_of_mass_squared:
		if distance_to_center_of_mass_squared < distance_max_squared:
			if attraction_direction.x == 0.0: attraction_direction.x = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.x, 2.0)
			if attraction_direction.y == 0.0: attraction_direction.y = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.y, 2.0)
			if distance_to_center_of_mass_squared < distance_min_squared: distance_to_center_of_mass_squared = sqrt(distance_min_squared * distance_to_center_of_mass_squared)
			graph_node.velocity += attraction_direction * tree_node.charge * alpha / distance_to_center_of_mass_squared
		return true
	
	if tree_node.is_branch() or distance_to_center_of_mass_squared >= distance_max_squared: return false
	
	if tree_node.leaves[0] != graph_node or tree_node.leaves.size() > 1:
		if attraction_direction.x == 0.0: attraction_direction.x = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.x, 2.0)
		if attraction_direction.y == 0.0: attraction_direction.y = jiggle(); distance_to_center_of_mass_squared += pow(attraction_direction.y, 2.0)
		if distance_to_center_of_mass_squared < distance_min_squared: distance_to_center_of_mass_squared = sqrt(distance_min_squared * distance_to_center_of_mass_squared)
	
	for leaf in tree_node.leaves:
		if leaf == graph_node: continue
		quad_width = graph_node.strength * alpha / distance_to_center_of_mass_squared
		graph_node.velocity += attraction_direction * quad_width
	
	return false

#endregion
#region Utils

func jiggle() -> float: return (_random.randf() - 0.5) * 1e-6

#endregion
#region Debug

func _mock() -> void:
	_mock_nodes()
	_mock_fixed_nodes()
	_mock_links()

func _mock_nodes() -> void:
	for n in mock_nodes_count: 
		var node := node_scene.instantiate() as ForceGraphNode
		nodes_container.add_child(node)

func _mock_fixed_nodes() -> void:
	for index in mock_fixed_nodes_count:
		var node := node_scene.instantiate() as ForceGraphNode
		node.fixed = true
		node.strength = -35.0
		node.place_node_spirally(index, node.initial_radius / 2.0, node.initial_angle / 2.0)
		nodes_container.add_child(node)

func _mock_links() -> void:
	for index in mock_link_count:
		var link := link_scene.instantiate() as ForceGraphLink
		link.source = index
		link.target = index + 4
		links_container.add_child(link)

func _draw() -> void:
	if draw_quad_tree or draw_center_of_mass: tree.visit(func(node: QuadTreeNode, rect: Rect2) -> bool:
		if draw_quad_tree: draw_rect(rect, Color.RED, false)
		if draw_center_of_mass: draw_circle(node.center_of_mass, 5.0, Color.GREEN)
		return false
	)

#endregion
