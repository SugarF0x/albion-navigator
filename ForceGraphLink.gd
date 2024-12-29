class_name ForceGraphLink extends Node2D

@onready var line: Line2D = $Line2D

@export var desired_distance := 30.0

var source := -1
var target := -1
var bias := 1.0
var strength := 1.0

func initialize(nodes: Array[ForceGraphNode]) -> void:
	initialize_connections_count(nodes)
	initialize_bias(nodes)
	initialize_strength(nodes)

func initialize_connections_count(nodes: Array[ForceGraphNode]) -> void:
	nodes[source].connections.append(target)
	nodes[target].connections.append(source)

func initialize_bias(nodes: Array[ForceGraphNode]) -> void:
	var source_connections_count := nodes[source].connections.size()
	var target_connections_count := nodes[target].connections.size()
	bias = source_connections_count / (source_connections_count + target_connections_count)

func initialize_strength(nodes: Array[ForceGraphNode]) -> void:
	strength = 1.0 / min(nodes[source].connections.size(), nodes[target].connections.size())

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
