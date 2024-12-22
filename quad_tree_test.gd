extends Node2D

var tree := QuadTree.new()

func _ready() -> void:
	tree.cover(Vector2(0, 0))
	tree.cover(Vector2(-1, -1))
	tree.cover(Vector2(1, -1))
	tree.cover(Vector2(1, 1))
	tree.cover(Vector2(-2, -2))

func _draw() -> void:
	var shifted_draw_rect := Rect2(tree.rect)
	shifted_draw_rect.position = Vector2(1, 1)
	draw_cell(tree.root, shifted_draw_rect)

func draw_cell(cell: QuadTreeNode, rect: Rect2) -> void:
	if cell.is_void():
		var expanded_rect := Rect2(rect)
		expanded_rect.position *= Vector2(50, 50)
		expanded_rect.size *= Vector2(50, 50)
		draw_rect(expanded_rect, Color.RED, false)
		return
	
	var sub_rect := Rect2(rect)
	sub_rect.size /= 2
	
	draw_cell(cell.nodes[QuadTreeNode.Quadrant.TOP_LEFT], move_rect(sub_rect, Vector2.ZERO))
	draw_cell(cell.nodes[QuadTreeNode.Quadrant.TOP_RIGHT], move_rect(sub_rect, Vector2(sub_rect.size.x, 0)))
	draw_cell(cell.nodes[QuadTreeNode.Quadrant.BOTTOM_LEFT], move_rect(sub_rect, Vector2(0, sub_rect.size.y)))
	draw_cell(cell.nodes[QuadTreeNode.Quadrant.BOTTOM_RIGHT], move_rect(sub_rect, sub_rect.size))

func move_rect(rect: Rect2, direction: Vector2) -> Rect2:
	var moved_rect := Rect2(rect)
	moved_rect.position += direction
	return moved_rect
