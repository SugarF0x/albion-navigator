extends TabBar

const LinkScene = preload("res://Entities/ZoneLink.tscn")

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ForceDirectedGraph
@onready var add_random_link_button: Button = $MarginContainer/VBoxContainer/AddRandomLinkButton
@onready var reheat_button: Button = $MarginContainer/VBoxContainer/ReheatButton

func _ready() -> void:
	add_random_link_button.pressed.connect(add_random_link)
	reheat_button.pressed.connect(graph.Reheat)

func add_random_link() -> void:
	var link := LinkScene.instantiate() as ZoneLink
	link.Connect(randi_range(410, 790), randi_range(410, 790))
	graph.AddLink(link)
