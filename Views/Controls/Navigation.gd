extends TabBar

const ZONE_GROUP := preload("res://Resources/ZoneGroup.tres")

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ZoneMap
@onready var shortest_route_from_input: AutoCompleteLineEdit = %ShortestRouteFromInput
@onready var shortest_route_to_input: AutoCompleteLineEdit = %ShortestRouteToInput
@onready var shortest_route_find_button: Button = %ShortestRouteFindButton
@onready var shortest_route_path_label: Label = %ShortestRoutePathLabel
@onready var shortest_route_copy_button: Button = %ShortestRouteCopyButton
@onready var shortest_route_clear_button: Button = %ShortestRouteClearButton
@onready var all_paths_out_input: AutoCompleteLineEdit = %AllPathsOutinput
@onready var all_paths_out_find_button: Button = %AllPathsOutFindButton
@onready var all_paths_out_to_royal_toggle: CheckButton = %AllPathsOutToRoyalToggle
@onready var all_paths_out_previous_button: Button = %AllPathsOutPreviousButton
@onready var all_paths_out_list_index_label: Label = %AllPathsOutListIndexLabel
@onready var all_paths_out_next_button: Button = %AllPathsOutNextButton
@onready var all_paths_out_path_label: Label = %AllPathsOutPathLabel
@onready var all_paths_out_copy_button: Button = %AllPathsOutCopyButton

var zones: Array[Zone] = []
var zone_names: Array[String] = []

enum HighlightType {
	Default,
	CityPortal,
	RoadToContinent,
	Path,
	WayOut,
}

enum ZoneType {
	StartingCity, 
	City, 
	SafeArea, 
	Yellow, 
	Red, 
	Black, 
	Road,
}

var zone_type_to_emoji_map: Dictionary = {
	ZoneType.StartingCity: ':house_with_garden:',
	ZoneType.City: ':european_castle:',
	ZoneType.SafeArea: ':blue_zone:',
	ZoneType.Yellow: ':yellow_zone:',
	ZoneType.Red: ':red_zone:',
	ZoneType.Black: ':black_zone:',
	ZoneType.Road: ':portal:'
  }

func _ready() -> void:
	register_zone_names()
	shortest_route_find_button.pressed.connect(find_shortest_path)
	shortest_route_clear_button.pressed.connect(clear_shortest_path)
	shortest_route_copy_button.pressed.connect(copy_shortest_path)
	all_paths_out_find_button.pressed.connect(find_all_paths_out)

func register_zone_names() -> void:
	zone_names.clear()
	zones.clear()
	
	for zone in ZONE_GROUP.load_all(): zones.append(zone as Zone)
	zones.sort_custom(func (a: Zone, b: Zone) -> bool: return a.Id < b.Id)
	
	var road_zone_names: Array[String] = []
	
	for zone in zones:
		zone_names.append(zone.DisplayName)
		if zone.Type == ZoneType.Road: road_zone_names.append(zone.DisplayName)
	
	shortest_route_from_input.options = zone_names
	shortest_route_to_input.options = zone_names
	all_paths_out_input.options = road_zone_names

func call_resize() -> void:
	# hacky solution to update tab size but it works
	(get_parent_control() as TabContainer).tab_changed.emit(-1)

var currently_selected_links: PackedInt32Array = [] :
	set(value):
		# TODO: reuse this logic for the all paths out display
		currently_selected_links = value
		if value.size() == 0:
			shortest_route_path_label.text = ""
			call_resize()
			return
		
		var first_link: ZoneLink = graph.Links[value[0]]
		var first_link_names: Array[int] = [first_link.Source, first_link.Target]
		var second_link: ZoneLink = graph.Links[value[mini(1, value.size() - 1)]]
		var second_link_names: Array[int] = [second_link.Source, second_link.Target]
		
		var node_indexes: Array[int] = []
		if value.size() > 1:
			node_indexes = first_link_names.filter(func (node_name: int) -> bool: return not second_link_names.has(node_name))
			for link_index in value:
				node_indexes.append(graph.Links[link_index].Target if node_indexes[maxi(0, node_indexes.size() - 1)] != graph.Links[link_index].Target else graph.Links[link_index].Source)
		else:
			node_indexes = first_link_names
		
		var nodes := node_indexes.map(func (i: int) -> ZoneNode: return graph.Nodes[i])
		var names := nodes.map(func (node: ZoneNode) -> String: return "{type} {name}".format({ "type": zone_type_to_emoji_map[node.Type], "name": node.DisplayName }))
		shortest_route_path_label.text = "\n".join(names)
		
		var min_expiration: String
		for i in value:
			var link: ZoneLink = graph.Links[i]
			var expiration: String = link.ExpiresAt
			# TODO: expiration might be miscalculated (timezone offset error), gotta check on that
			if expiration.is_empty(): continue
			if min_expiration.is_empty(): min_expiration = expiration; continue
			
			var expiration_date := Time.get_unix_time_from_datetime_string(expiration)
			var min_expiration_date := Time.get_unix_time_from_datetime_string(min_expiration)
			
			min_expiration = min_expiration if min_expiration_date < expiration_date else expiration
		
		if not min_expiration.is_empty():
			shortest_route_path_label.text = "Expires: <t:{expiration}:R>\n{rest}".format({ "rest": shortest_route_path_label.text, "expiration": Time.get_unix_time_from_datetime_string(min_expiration) })
		
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

func copy_shortest_path() -> void:
	DisplayServer.clipboard_set(shortest_route_path_label.text)

func find_all_paths_out() -> void:
	var from := all_paths_out_input.get_value()
	if from.is_empty(): return
	
	var zone_index := zone_names.find(from)
	if zone_index < 0 or zone_index >= zones.size(): return
	
	var all_paths_out: Array[Array] = graph.FindAllPathsOut(zone_index, all_paths_out_to_royal_toggle.button_pressed)
	print(zone_index, " ", zones[zone_index].DisplayName)
	print(all_paths_out)
	# i forgor these are actually link indexes, not node indexes :skull:
	#print(all_paths_out.map(func (path: Array) -> Array: return path.map(func (id: int) -> String: return zones[id].DisplayName)))
	
