class_name QuadTreeNode extends RefCounted

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

var leaves: Array[ForceGraphNode] = []
var branches: Array[QuadTreeNode] = []

func attach_leaf(leaf: ForceGraphNode) -> void:
	if is_branch():
		push_error('Cant attach leaf: not a branch')
		return
	
	leaves.append(leaf)

func branch_out(quadrant: Quadrant = -1) -> void:
	if is_branch():
		push_error('Cant branch out: already a branch')
		return
	
	if is_leaf():
		if quadrant < 0:
			push_error('Cant branch out: existing leaf requires quadrant to be assigned')
			return
		
		create_empty_branches()
		for leaf in leaves: 
			branches[quadrant].attach_leaf(leaf)
			
		leaves.clear()
		return
	
	create_empty_branches()

func remove_leaf(target_leaf: ForceGraphNode) -> void:
	if not is_leaf():
		push_error('Cant remove leaf: not a leaf')
		return
	
	var leaf_index := leaves.find(target_leaf)
	if leaf_index < 0: return
	
	leaves.remove_at(leaf_index)

func create_empty_branches() -> void:
	branches.clear()
	for quadrant in Quadrant.values() as Array[int]: branches.append(QuadTreeNode.new())

func trim_branches() -> void:
	if not is_branch():
		push_error('Cant trim branches: not a branch')
		return
	
	var populated_branches: Array[QuadTreeNode] = branches.filter(func (node: QuadTreeNode) -> bool: return not node.is_void())
	if populated_branches.size() > 1: return
	
	if populated_branches.size() == 1:
		var only_branch := populated_branches[0]
		if not only_branch.is_leaf(): return
		
		branches.clear()
		for leaf in only_branch.leaves: attach_leaf(leaf)
		return
	
	branches.clear()

func get_type() -> Type:
	if leaves.size(): return Type.LEAF
	if branches.size(): return Type.BRANCH
	return Type.VOID

func is_void() -> bool: return get_type() == Type.VOID
func is_branch() -> bool: return get_type() == Type.BRANCH
func is_leaf() -> bool: return get_type() == Type.LEAF

func _to_string() -> String: 
	return "QuadTreeNode(type: {type}, leaves: {leaves}, branches: {branches})".format({
			"type": Type.find_key(get_type()),
			"leaves": leaves.size(),
			"branches": branches.size(),
		})
