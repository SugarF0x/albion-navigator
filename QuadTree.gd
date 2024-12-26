class_name QuadTree extends RefCounted

var rect := Rect2()
var root := QuadTreeNode.new()

const Quadrant := Quad.Quadrant

func add(node: ForceGraphNode, cover := true) -> QuadTree:
	if cover: cover(node.position)
	
	if root.is_void():
		root.attach_leaf(node)
		return self
	
	var explored_node := root
	var explored_rect := Rect2(rect)
	var explored_node_parent: QuadTreeNode
	var explored_node_quadrant: Quadrant
	
	while explored_node.is_branch():
		var mid_point := explored_rect.get_center()
		explored_node_quadrant = Quad.get_relative_quadrant(mid_point, node.position)
		explored_rect = Quad.shrink_rect_to_quadrant(explored_rect, explored_node_quadrant)
		explored_node_parent = explored_node
		explored_node = explored_node.branches[explored_node_quadrant]
	
	if explored_node.is_void():
		explored_node.attach_leaf(node)
		return self
	
	if node.position.is_equal_approx(explored_node.leaves[0].position):
		explored_node.attach_leaf(node)
		return self
	
	while true:
		var mid_point := explored_rect.get_center()
		var old_node_quadrant := Quad.get_relative_quadrant(mid_point, explored_node.leaves[0].position)
		var new_node_quadrant := Quad.get_relative_quadrant(mid_point, node.position)
		
		if old_node_quadrant != new_node_quadrant:
			explored_node.branch_out(old_node_quadrant)
			explored_node.branches[new_node_quadrant].attach_leaf(node)
			break
		
		explored_node.branch_out(old_node_quadrant)
		explored_node = explored_node.branches[old_node_quadrant]
		explored_rect = Quad.shrink_rect_to_quadrant(explored_rect, new_node_quadrant)
	
	return self

func add_all(nodes: Array[ForceGraphNode]) -> QuadTree:
	if not nodes.size(): return self
	if nodes.size() == 1: return add(nodes[0])
	
	var first_node: ForceGraphNode = nodes.pop_back()
	var bounding_box := Rect2(first_node.position, Vector2.ZERO)
	
	for node in nodes:
		bounding_box.position = bounding_box.position.min(node.position)
		bounding_box.end = bounding_box.end.max(node.position)
	
	cover(bounding_box.position).cover(bounding_box.end)
	for node in nodes: add(node, false)
	
	return self

func remove(node: ForceGraphNode) -> QuadTree:
	if root.is_void(): return self
	
	var explored_node := root
	var explored_rect := Rect2(rect)
	var explored_node_parent_stack: Array[QuadTreeNode]
	var explored_node_quadrant: Quadrant
	
	while explored_node.is_branch():
		var mid_point := explored_rect.get_center()
		explored_node_quadrant = Quad.get_relative_quadrant(mid_point, node.position)
		explored_rect = Quad.shrink_rect_to_quadrant(explored_rect, explored_node_quadrant)
		explored_node_parent_stack.append(explored_node)
		explored_node = explored_node.branches[explored_node_quadrant]
	
	## return if node is not in leaves
	if explored_node.is_void(): return self
	
	## find if node is in leaf stack and return if not
	var target_node_index := explored_node.leaves.find(node)
	if target_node_index < 0: return self
	
	## remove node and exit if there are other leaves left
	explored_node.leaves.remove_at(target_node_index)
	if explored_node.is_leaf(): return self
	
	## trim branches upstream as long as branches are void
	while true:
		explored_node = explored_node_parent_stack.pop_back()
		if not explored_node: break
		explored_node.trim_branches()
		if not explored_node.is_void(): break
	
	return self

func remove_all(nodes: Array[ForceGraphNode]) -> QuadTree:
	for node in nodes: remove(node)
	return self

func cover(point: Vector2) -> QuadTree:
	if not rect.has_area():
		rect.position = Vector2(floor(point.x), floor(point.y))
		rect = rect.grow_individual(0,0,1,1)
		return self
	
	var z := rect.size.x
	var node := root
	
	while not rect.has_point(point):
		var quadrant := Quad.get_relative_quadrant(point, rect.position)
		var parent := QuadTreeNode.new()
		parent.create_empty_branches()
		parent.branches[quadrant] = node
		node = parent
		rect.size *= 2
		match quadrant:
			Quadrant.TOP_LEFT: pass
			Quadrant.TOP_RIGHT: rect.position.x -= z;
			Quadrant.BOTTOM_LEFT: rect.position.y -= z;
			Quadrant.BOTTOM_RIGHT: rect.position -= Vector2(z, z);
	
	root = node
	return self

func visit_after(callback: Callable) -> QuadTree:
	var quads: Array[Quad] = []
	var next: Array[Quad] = []
	
	if not root.is_void(): quads.append(Quad.new(root, rect))
	
	while true:
		var quad: Quad = quads.pop_back()
		if not quad: break
		
		if quad.node.is_void(): continue
		next.append(quad)
		
		for branch_quadrant in quad.node.branches.size() as Quadrant:
			var branch := quad.node.branches[branch_quadrant]
			if branch.is_void(): continue
			quads.append(Quad.new(branch, Quad.shrink_rect_to_quadrant(rect, branch_quadrant)))
	
	next.reverse()
	for quad in next: callback.call(quad.node, quad.rect)
	return self

func visit(callback: Callable) -> QuadTree:
	var quads: Array[Quad] = []
	
	if not root.is_void(): quads.append(Quad.new(root, rect))
	
	while true:
		var quad: Quad = quads.pop_back()
		if not quad: break
		
		if quad.node.is_void(): continue
		if callback.call(quad.node, quad.rect): continue
		
		for branch_quadrant in range(quad.node.branches.size() - 1, -1, -1) as Array[Quadrant]:
			var branch := quad.node.branches[branch_quadrant]
			if branch.is_void(): continue
			quads.append(Quad.new(branch, Quad.shrink_rect_to_quadrant(quad.rect, branch_quadrant)))
	
	return self

func _to_string() -> String:
	return "QuadTree({rect}, {root})".format({ "rect": rect, "root": root })

func _to_dict() -> Dictionary:
	return {
		"rect": rect,
		"root": root._to_dict()
	}
