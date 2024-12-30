class_name Quad extends RefCounted

enum Quadrant {
	TOP_LEFT,
	TOP_RIGHT,
	BOTTOM_LEFT,
	BOTTOM_RIGHT,
}

var node: QuadTreeNode
var rect: Rect2

func _init(initial_node: QuadTreeNode, initial_rect: Rect2) -> void:
	node = initial_node
	rect = initial_rect

## Returns target quadrant relative to source
static func get_relative_quadrant(source: Vector2, target: Vector2) -> Quadrant:
	return int(target.y >= source.y) << 1 | int(target.x >= source.x) as Quadrant

## Half the quadrant size and set position to respective quadrant -> returns new rect
static func shrink_rect_to_quadrant(rect_to_shrink: Rect2, quadrant: Quadrant) -> Rect2:
	var new_rect := Rect2(rect_to_shrink)
	new_rect.size /= 2
	match quadrant:
		Quadrant.TOP_LEFT: pass
		Quadrant.TOP_RIGHT: new_rect.position.x += new_rect.size.x
		Quadrant.BOTTOM_LEFT: new_rect.position.y += new_rect.size.y
		Quadrant.BOTTOM_RIGHT: new_rect.position += new_rect.size
	return new_rect

## Expand the rect in the direction opposite of the given quadrant (quadrant argument is quadrant origin) -> returns new rect
static func expand_rect_from_quadrant(rect_to_expand: Rect2, quadrant: Quadrant) -> Rect2:
	var new_rect := Rect2(rect_to_expand)
	var offset := new_rect.size.x
	new_rect.size *= 2
	match quadrant:
		Quadrant.TOP_LEFT: pass
		Quadrant.TOP_RIGHT: new_rect.position.x -= offset
		Quadrant.BOTTOM_LEFT: new_rect.position.y -= offset
		Quadrant.BOTTOM_RIGHT: new_rect.position -= Vector2(offset, offset)
	return new_rect
