extends TabBar

const LinkScene = preload("res://Entities/ZoneLink.tscn")

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ForceDirectedGraph

@onready var reheat_button: Button = %ReheatButton
@onready var add_sample_data_button: Button = %AddSampleDataButton
@onready var flush_data_button: Button = %FlushDataButton

func _ready() -> void:
	reheat_button.pressed.connect(graph.Reheat)
	add_sample_data_button.pressed.connect(PersistedStore.LoadSampleData)
	
	add_sample_data_button.visible = OS.is_debug_build()
	flush_data_button.visible = OS.is_debug_build()
	
	# TODO: remove all links that have expiry
	# flush_data_button.pressed.connect()
