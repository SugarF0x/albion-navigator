extends TabBar

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ZoneMap
@onready var shortest_route_from_input: AutoCompleteLineEdit = %ShortestRouteFromInput
@onready var shortest_route_to_input: AutoCompleteLineEdit = %ShortestRouteToInput
@onready var shortest_route_find_button: Button = %ShortestRouteFindButton
@onready var shortest_route_path_label: Label = %ShortestRoutePathLabel
@onready var shortest_route_copy_button: Button = %ShortestRouteCopyButton
@onready var all_paths_outinput: AutoCompleteLineEdit = %AllPathsOutinput
@onready var all_paths_out_find_button: Button = %AllPathsOutFindButton
@onready var all_paths_out_previous_button: Button = %AllPathsOutPreviousButton
@onready var all_paths_out_list_index_label: Label = %AllPathsOutListIndexLabel
@onready var all_paths_out_next_button: Button = %AllPathsOutNextButton
@onready var all_paths_out_path_label: Label = %AllPathsOutPathLabel
@onready var all_paths_out_copy_button: Button = %AllPathsOutCopyButton

func _ready() -> void:
	graph.ChildrenRegistered.connect(register_zone_names)

func register_zone_names(nodes: Array, _links: Array) -> void:
	var zone_names: Array[String]
	var road_zone_names: Array[String]
	
	for node: ForceGraphNode in nodes:
		if node is not ZoneNode: continue
		if node.DisplayName == "": continue
		zone_names.append(node.DisplayName)
		if node.Type == 6: road_zone_names.append(node.DisplayName)
	
	shortest_route_from_input.options = zone_names
	shortest_route_to_input.options = zone_names
	all_paths_outinput.options = road_zone_names
