extends TabBar

# TODO: perhaps use godot line edit auto-complete plugin (or not, it kinda looks like ass, better justunderlay text under it)
const DUMMY_LINK := preload("res://DummyLink.tscn")

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ForceDirectedGraph
@onready var register_button: Button = $MarginContainer/HBoxContainer/VBoxContainer/RegisterButton
@onready var source_zone_edit: LineEdit = $MarginContainer/HBoxContainer/VBoxContainer/SourceZoneEdit
@onready var target_zone_edit: LineEdit = $MarginContainer/HBoxContainer/VBoxContainer/TargetZoneEdit

func _ready() -> void:
	register_button.pressed.connect(register_new_link)

func register_new_link() -> void:
	var nodes := graph.Nodes as Array
	var source: int = -1
	var target: int = -1
	
	for node: ZoneNode in nodes:
		if node.DisplayName == source_zone_edit.text: source = node.Index
		if node.DisplayName == target_zone_edit.text: target = node.Index
		if source > 0 and target > 0: break
	
	if source < 0 or target < 0: return
	
	var link := DUMMY_LINK.instantiate() as ForceGraphLink
	link.Connect(source, target)
	graph.AddLink(link)
