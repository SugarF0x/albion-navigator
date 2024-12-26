extends Node2D

@onready var v_box_container: VBoxContainer = $CanvasLayer/Control/PanelContainer/MarginContainer/VBoxContainer

var BUTTON_TEXT_TO_CALLBACK_MAP: Dictionary = {
	"Add random node": add_new_node,
	"Add a dozen random nodes": add_dozen_random_nodes,
	"Remove last node": remove_last_node,
	"Remove all nodes": remove_all_nodes,
	"Log visit after": run_visit_after,
	"Log visit": run_visit,
	"Log data": log_data,
	"To JSON": log_tree_as_json,
}

var tree := QuadTree.new()
var start_offset := Vector2.ZERO
var nodes: Array[ForceGraphNode] = []

const SIZE_MULTIPLIER := 50
const VECTOR_MULTIPLIER := Vector2(SIZE_MULTIPLIER, SIZE_MULTIPLIER)

func _ready() -> void:
	for key: String in BUTTON_TEXT_TO_CALLBACK_MAP:
		var button := Button.new()
		button.text = key
		button.pressed.connect(BUTTON_TEXT_TO_CALLBACK_MAP[key])
		v_box_container.add_child(button)

#region draw

func _draw() -> void:
	draw_circle(start_offset * VECTOR_MULTIPLIER, 5.0, Color.GREEN)
	draw_branch(tree.root, tree.rect)

func redraw() -> void:
	start_offset = Vector2.ONE - tree.rect.position
	queue_redraw()

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
	if cell.is_leaf(): return draw_leaf(cell.leaves[0])
	
	var sub_rect := Rect2(rect)
	sub_rect.size /= 2
	
	draw_branch(cell.branches[QuadTreeNode.Quadrant.TOP_LEFT], move_rect(sub_rect, Vector2.ZERO))
	draw_branch(cell.branches[QuadTreeNode.Quadrant.TOP_RIGHT], move_rect(sub_rect, Vector2(sub_rect.size.x, 0)))
	draw_branch(cell.branches[QuadTreeNode.Quadrant.BOTTOM_LEFT], move_rect(sub_rect, Vector2(0, sub_rect.size.y)))
	draw_branch(cell.branches[QuadTreeNode.Quadrant.BOTTOM_RIGHT], move_rect(sub_rect, sub_rect.size))

#endregion
#region util

func move_rect(rect: Rect2, direction: Vector2) -> Rect2:
	var moved_rect := Rect2(rect)
	moved_rect.position += direction
	return moved_rect

#endregion
#region controls

func add_new_node() -> void:
	var node := ForceGraphNode.new(Vector2(randf_range(-2, 2), randf_range(-2, 2)))
	print("Adding new node at: ", node.position)
	tree.add(node)
	nodes.append(node)
	redraw()

func add_dozen_random_nodes() -> void:
	var new_nodes: Array[ForceGraphNode] = []
	for n in 12: new_nodes.append(ForceGraphNode.new(Vector2(randf_range(-2, 2), randf_range(-2, 2))))
	tree.add_all(new_nodes)
	nodes.append_array(new_nodes)
	redraw()

func remove_last_node() -> void:
	var node_to_remove: ForceGraphNode = nodes.pop_back()
	if not node_to_remove: return
	
	tree.remove(node_to_remove)
	redraw()

func remove_all_nodes() -> void:
	tree.remove_all(nodes)
	redraw()

func log_node(node: QuadTreeNode, rect: Rect2) -> void:
	print(node, ' ', rect)

func run_visit_after() -> void:
	tree.visit_after(log_node)

func log_only_negative_node(node: QuadTreeNode, rect: Rect2) -> bool:
	var is_negative: bool = rect.position.x < 0 and rect.position.y < 0
	print("Visited ", node.Type.find_key(node.get_type()), " ", rect, " | It is ", "negative - proceeding" if is_negative else "positive - aborting")
	return not is_negative

func run_visit() -> void:
	tree.visit(log_only_negative_node)

func log_tree_as_json() -> void:
	print(JSON.stringify(tree._to_dict(), "  "))

func log_data() -> void:
	print(JSON.stringify(tree.get_data().map(func(node: ForceGraphNode) -> Dictionary: return node._to_dict()), "  "))

#endregion
