class_name QuadTreeNode extends Object

enum Type {
	VOID,
	BRANCH,
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

func set_leaf_data(value: ForceGraphNode) -> void:
	data = value
	type = Type.LEAF

func create_empty_branch() -> void:
	nodes.clear()
	for quadrant in Quadrant.values() as Array[int]: nodes.append(QuadTreeNode.new())
	type = Type.BRANCH

func set_node(quadrant: Quadrant, value: QuadTreeNode) -> void:
	if not is_branch():
		push_error('Cant set node: not a branch')
		return
	
	nodes[quadrant] = value

func branch_off(quadrant: Quadrant) -> void:
	if not is_leaf():
		push_error("Cant branch off: not a leaf")
		return
	
	create_empty_branch()
	nodes[quadrant].set_leaf_data(data)

func set_nodes(value: Array[QuadTreeNode]) -> void:
	nodes = value
	type = Type.BRANCH

func is_void() -> bool: return type == Type.VOID
func is_branch() -> bool: return type == Type.BRANCH
func is_leaf() -> bool: return type == Type.LEAF

func _to_string() -> String:
	return "QuadTreeNode(%d)" % [type]
