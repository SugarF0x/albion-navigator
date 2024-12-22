class_name QuadTreeNode extends Object

enum Type {
	VOID,
	NODE,
	LEAF,
}

enum Quadrant {
	TOP_LEFT,
	TOP_RIGHT,
	BOTTOM_LEFT,
	BOTTOM_RIGHT,
}

var data: ForceGraphNode
var nodes: Array[QuadTreeNode] = []
var type: Type = Type.VOID

func set_data(value: ForceGraphNode) -> void:
	data = value
	type = Type.LEAF

func create_nodes() -> void:
	nodes.clear()
	for quadrant in Quadrant.values() as Array[int]: nodes.append(QuadTreeNode.new())
	type = Type.NODE

func set_nodes(value: Array[QuadTreeNode]) -> void:
	nodes = value
	type = Type.NODE

func is_void() -> bool: return type == Type.VOID

func _to_string() -> String:
	return "QuadTreeNode(%d)" % [type]
