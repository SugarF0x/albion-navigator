extends TabBar

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ZoneMap
@onready var shortest_route_from_input: AutoCompleteLineEdit = %ShortestRouteFromInput
@onready var shortest_route_to_input: AutoCompleteLineEdit = %ShortestRouteToInput
@onready var shortest_route_find_button: Button = %ShortestRouteFindButton
@onready var shortest_route_path_label: Label = %ShortestRoutePathLabel
@onready var shortest_route_copy_button: Button = %ShortestRouteCopyButton
@onready var shortest_route_clear_button: Button = %ShortestRouteClearButton
@onready var all_paths_outinput: AutoCompleteLineEdit = %AllPathsOutinput
@onready var all_paths_out_find_button: Button = %AllPathsOutFindButton
@onready var all_paths_out_to_royal_toggle: CheckButton = %AllPathsOutToRoyalToggle
@onready var all_paths_out_previous_button: Button = %AllPathsOutPreviousButton
@onready var all_paths_out_list_index_label: Label = %AllPathsOutListIndexLabel
@onready var all_paths_out_next_button: Button = %AllPathsOutNextButton
@onready var all_paths_out_path_label: Label = %AllPathsOutPathLabel
@onready var all_paths_out_copy_button: Button = %AllPathsOutCopyButton

var zone_names: Array[String] = []

enum HighlightType {
	Default,
	CityPortal,
	RoadToContinent,
	Path,
	WayOut,
}

func _ready() -> void:
	graph.ChildrenRegistered.connect(register_zone_names)
	shortest_route_find_button.pressed.connect(find_shortest_path)
	shortest_route_clear_button.pressed.connect(clear_shortest_path)

func register_zone_names(nodes: Array, _links: Array) -> void:
	zone_names.clear()
	var road_zone_names: Array[String]
	
	for node: ForceGraphNode in nodes:
		if node is not ZoneNode: continue
		if node.DisplayName == "": continue
		zone_names.append(node.DisplayName)
		if node.Type == 6: road_zone_names.append(node.DisplayName)
	
	shortest_route_from_input.options = zone_names
	shortest_route_to_input.options = zone_names
	all_paths_outinput.options = road_zone_names

func call_resize() -> void:
	# hacky solution to update tab size but it works
	(get_parent_control() as TabContainer).tab_changed.emit(-1)

var currently_selected_links: PackedInt32Array = [] :
	set(value):
		currently_selected_links = value
		if value.size() == 0:
			shortest_route_path_label.text = ""
			call_resize()
			return
		
		var node_indexes: Array[int] = [graph.Links[value[0]].Source]
		for link_index in value:
			node_indexes.append(graph.Links[link_index].Target)
			
		var nodes := node_indexes.map(func (i: int) -> ZoneNode: return graph.Nodes[i])
		var names := nodes.map(func (node: ZoneNode) -> String: return node.DisplayName)
		shortest_route_path_label.text = "\n".join(names)
		
		call_resize()

func find_shortest_path() -> void:
	graph.HighlightLinks(currently_selected_links)
	
	var from := shortest_route_from_input.get_value()
	var to := shortest_route_to_input.get_value()
	
	if from == "" or to == "": return
	
	var from_index := zone_names.find(from)
	var to_index := zone_names.find(to)
	
	currently_selected_links = graph.FindShortestPath(from_index, to_index)
	graph.HighlightLinks(currently_selected_links, HighlightType.Path)

func clear_shortest_path() -> void:
	graph.HighlightLinks(currently_selected_links)
	currently_selected_links = []
