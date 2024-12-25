class_name QuadTree extends RefCounted

var rect := Rect2()
var root := QuadTreeNode.new()

const Quadrant = QuadTreeNode.Quadrant

## Returns target quadrant relative to source
func get_relative_quadrant(source: Vector2, target: Vector2) -> Quadrant:
	if target.y < source.y:
		if target.x < source.x: return Quadrant.TOP_LEFT
		return Quadrant.TOP_RIGHT
	if target.x < source.x: return Quadrant.BOTTOM_LEFT
	return Quadrant.BOTTOM_RIGHT

static func shrink_rect_to_quadrant(rect: Rect2, quadrant: Quadrant) -> Rect2:
	var new_rect := Rect2(rect)
	new_rect.size /= 2
	match quadrant:
		Quadrant.TOP_LEFT: pass
		Quadrant.TOP_RIGHT: new_rect.position.x += new_rect.size.x
		Quadrant.BOTTOM_LEFT: new_rect.position.y += new_rect.size.y
		Quadrant.BOTTOM_RIGHT: new_rect.position += new_rect.size
	return new_rect

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
		explored_node_quadrant = get_relative_quadrant(mid_point, node.position)
		explored_rect = shrink_rect_to_quadrant(explored_rect, explored_node_quadrant)
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
		var old_node_quadrant := get_relative_quadrant(mid_point, explored_node.leaves[0].position)
		var new_node_quadrant := get_relative_quadrant(mid_point, node.position)
		
		if old_node_quadrant != new_node_quadrant:
			explored_node.branch_out(old_node_quadrant)
			explored_node.branches[new_node_quadrant].attach_leaf(node)
			break
		
		explored_node.branch_out(old_node_quadrant)
		explored_node = explored_node.branches[old_node_quadrant]
		explored_rect = shrink_rect_to_quadrant(explored_rect, new_node_quadrant)
	
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

func cover(point: Vector2) -> QuadTree:
	if not rect.has_area():
		rect.position = Vector2(floor(point.x), floor(point.y))
		rect = rect.grow_individual(0,0,1,1)
		return self
	
	var z := rect.size.x
	var node := root
	
	while not rect.has_point(point):
		var quadrant := get_relative_quadrant(point, rect.position)
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

func _to_string() -> String:
	return "QuadTree({rect}, {root})".format({ "rect": rect, "root": root })
