@tool
class_name QuadTree extends Object

var rect := Rect2()
var root := QuadTreeNode.new()

const Quadrant = QuadTreeNode.Quadrant

#func _run() -> void:
	#print('---- ', Time.get_datetime_string_from_system())
	#var foo := QuadTree.new()
	#foo.cover(Vector2(0, 0))
	#foo.cover(Vector2(-1.7, 1.3))
	#foo.cover(Vector2(2, 2))
	#foo.cover(Vector2(1, 1))
	#print(foo.root.nodes[0].nodes[1].nodes)

func get_relative_quadrant(source: Vector2, target: Vector2) -> Quadrant:
	if target.y < source.y:
		if target.x < source.x: return Quadrant.TOP_LEFT
		return Quadrant.TOP_RIGHT
	if target.x < source.x: return Quadrant.BOTTOM_LEFT
	return Quadrant.BOTTOM_RIGHT

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
		parent.create_nodes()
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
