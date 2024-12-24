@tool
class_name QuadTree extends Object

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
		root.set_leaf_data(node)
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
		explored_node = explored_node.nodes[explored_node_quadrant]
	
	if explored_node.is_void():
		explored_node.set_leaf_data(node)
		return self
	
	if node.position.is_equal_approx(explored_node.data.position):
		var new_leaf := QuadTreeNode.new()
		new_leaf.set_leaf_data(node)
		new_leaf.attach_next_leaf(explored_node)
		# TODO: fix this, crashed if the first two nodes are same nodes
		explored_node_parent.nodes[explored_node_quadrant] = new_leaf
		return self
	
	while true:
		var mid_point := explored_rect.get_center()
		var old_node_quadrant := get_relative_quadrant(mid_point, explored_node.data.position)
		var new_node_quadrant := get_relative_quadrant(mid_point, node.position)
		
		if old_node_quadrant != new_node_quadrant:
			explored_node.branch_off(old_node_quadrant)
			explored_node.nodes[new_node_quadrant].set_leaf_data(node)
			break
		
		explored_node.branch_off(old_node_quadrant)
		explored_node = explored_node.nodes[old_node_quadrant]
		explored_rect = shrink_rect_to_quadrant(explored_rect, new_node_quadrant)
	
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
		parent.create_empty_branch()
		parent.nodes[quadrant] = node
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
