extends Node2D

var tree := QuadTree.new()
var start_offset := Vector2.ZERO

const SIZE_MULTIPLIER := 50
const VECTOR_MULTIPLIER := Vector2(SIZE_MULTIPLIER, SIZE_MULTIPLIER)

func _ready() -> void:
	tree.add(ForceGraphNode.new(Vector2(-0.5, -0.5)))
	tree.add(ForceGraphNode.new(Vector2(0.5, -0.5)))
	tree.add(ForceGraphNode.new(Vector2(1.5, 1.5)))
	tree.add(ForceGraphNode.new(Vector2(-1.5, -1.5)))
	tree.add(ForceGraphNode.new(Vector2(-2.5, -2.5)))
	start_offset = Vector2.ONE - tree.rect.position

func _draw() -> void:
	draw_circle(start_offset * VECTOR_MULTIPLIER, 5.0, Color.GREEN)
	draw_branch(tree.root, tree.rect)

func draw_void(rect: Rect2) -> void:
	var expanded_rect := Rect2(rect)
	expanded_rect.position += start_offset
	expanded_rect.position *= VECTOR_MULTIPLIER
	expanded_rect.size *= VECTOR_MULTIPLIER
	draw_rect(expanded_rect, Color.RED, false)

func draw_leaf(node: ForceGraphNode) -> void:
	draw_circle((node.position + start_offset) * VECTOR_MULTIPLIER, 5.0, Color.BLUE)
	return

func draw_branch(cell: QuadTreeNode, rect: Rect2) -> void:
	draw_void(rect)
	if cell.is_void(): return draw_void(rect)
	if cell.is_leaf(): return draw_leaf(cell.data)
	
	var sub_rect := Rect2(rect)
	sub_rect.size /= 2
	
	draw_branch(cell.nodes[QuadTreeNode.Quadrant.TOP_LEFT], move_rect(sub_rect, Vector2.ZERO))
	draw_branch(cell.nodes[QuadTreeNode.Quadrant.TOP_RIGHT], move_rect(sub_rect, Vector2(sub_rect.size.x, 0)))
	draw_branch(cell.nodes[QuadTreeNode.Quadrant.BOTTOM_LEFT], move_rect(sub_rect, Vector2(0, sub_rect.size.y)))
	draw_branch(cell.nodes[QuadTreeNode.Quadrant.BOTTOM_RIGHT], move_rect(sub_rect, sub_rect.size))

func move_rect(rect: Rect2, direction: Vector2) -> Rect2:
	var moved_rect := Rect2(rect)
	moved_rect.position += direction
	return moved_rect
