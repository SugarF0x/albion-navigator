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
	OutlandCity,
}

# TODO: this should probably come from settings
var zone_type_to_emoji_map: Dictionary = {
	ZoneType.StartingCity: ':house_with_garden:',
	ZoneType.City: ':european_castle:',
	ZoneType.SafeArea: ':blue_zone:',
	ZoneType.Yellow: ':yellow_zone:',
	ZoneType.Red: ':red_zone:',
	ZoneType.Black: ':black_zone:',
	ZoneType.Road: ':portal:',
	ZoneType.OutlandCity: ':house_abandoned:',
  }

func _ready() -> void:
	register_zone_names()
	shortest_route_find_button.pressed.connect(find_shortest_path)
	shortest_route_clear_button.pressed.connect(clear_shortest_path)
	shortest_route_copy_button.pressed.connect(copy_shortest_path)
	all_paths_out_find_button.pressed.connect(find_all_paths_out)
	all_paths_out_copy_button.pressed.connect(copy_shortest_path_out)
	all_paths_out_previous_button.pressed.connect(func() -> void: selected_path_out_index = maxi(selected_path_out_index - 1, 0))
	all_paths_out_next_button.pressed.connect(func() -> void: selected_path_out_index = mini(selected_path_out_index + 1, all_paths_out.size() - 1))
	all_paths_out_previous_button.disabled = true
	all_paths_out_next_button.disabled = true

func register_zone_names() -> void:
	zone_names.clear()
	zones.clear()
	
	for zone in ZONE_GROUP.load_all(): zones.append(zone as Zone)
	zones.sort_custom(func (a: Zone, b: Zone) -> bool: return a.Id < b.Id)
	
	for zone in zones: zone_names.append(zone.DisplayName)
	
	shortest_route_from_input.options = zone_names
	shortest_route_to_input.options = zone_names
	all_paths_out_input.options = zone_names

func call_resize() -> void:
	# hacky solution to update tab size but it works
	(get_parent_control() as TabContainer).tab_changed.emit(-1)

func get_min_links_expiration(links: PackedInt32Array) -> String:
	var min_expiration: String
	for i in links:
		var link: ZoneLink = graph.Links[i]
		var expiration: String = link.ExpiresAt
		if expiration.is_empty(): continue
		if min_expiration.is_empty(): min_expiration = expiration; continue
		
		var expiration_date := Time.get_unix_time_from_datetime_string(expiration)
		var min_expiration_date := Time.get_unix_time_from_datetime_string(min_expiration)
		
		min_expiration = min_expiration if min_expiration_date < expiration_date else expiration
	
	return min_expiration

func get_path_string_in_discord_format(zone_indexes: Array[int], link_indexes: PackedInt32Array) -> String:
	if zone_indexes.size() == 0: return ""
	
	var nodes := zone_indexes.map(func (i: int) -> ZoneNode: return graph.Nodes[i])
	var names := nodes.map(func (node: ZoneNode) -> String: return "{type} {name}".format({ "type": zone_type_to_emoji_map[node.Type], "name": node.DisplayName }))
	var route_stack := "\n".join(names)
	
	var min_expiration := get_min_links_expiration(link_indexes)
	if min_expiration.is_empty(): return route_stack
	return "Expires: <t:{expiration}:R>\n{rest}".format({ "rest": route_stack, "expiration": Time.get_unix_time_from_datetime_string(min_expiration) })

var currently_selected_path_node_indexes: Array[int] = []
var currently_selected_links: PackedInt32Array = [] :
	set(value):
		currently_selected_links = value
		currently_selected_path_node_indexes.clear()
		if value.size() == 0: return
		
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
		
		currently_selected_path_node_indexes = node_indexes

func find_shortest_path() -> void:
	graph.HighlightLinks(currently_selected_links)
	
	var from := shortest_route_from_input.get_value()
	var to := shortest_route_to_input.get_value()
	
	if from == "" or to == "": return
	
	var from_index := zone_names.find(from)
	var to_index := zone_names.find(to)
	
	currently_selected_links = graph.FindShortestPath(from_index, to_index)
	if currently_selected_links.size() == 0: 
		call_resize()
		return
	
	graph.HighlightLinks(currently_selected_links, HighlightType.Path)
	shortest_route_path_label.text = get_path_string_in_discord_format(currently_selected_path_node_indexes, currently_selected_links)
	call_resize()

func clear_shortest_path() -> void:
	graph.HighlightLinks(currently_selected_links)
	currently_selected_links = []
	shortest_route_path_label.text = ""
	call_resize()

func copy_shortest_path() -> void:
	if currently_selected_links.size() == 0: return
	DisplayServer.clipboard_set(shortest_route_path_label.text)

func copy_shortest_path_out() -> void:
	if currently_selected_links.size() == 0: return
	DisplayServer.clipboard_set(all_paths_out_path_label.text)

func update_all_paths_out_label() -> void:
	all_paths_out_list_index_label.text = "{index}/{length}".format({ "index": selected_path_out_index + 1, "length": all_paths_out.size() })

var all_paths_out: Array[Array] = [] : 
	set(value):
		all_paths_out = value
		update_all_paths_out_label()

var selected_path_out_index := -1 :
	set(value):
		if value == selected_path_out_index: return
		graph.HighlightLinks(currently_selected_links)
		all_paths_out_previous_button.disabled = value <= 0
		all_paths_out_next_button.disabled = value >= all_paths_out.size() - 1
		
		selected_path_out_index = value
		update_all_paths_out_label()
		if value < 0: 
			all_paths_out_path_label.text = ""
			call_resize()
			return
		
		currently_selected_links = all_paths_out[value]
		graph.HighlightLinks(all_paths_out[value], HighlightType.Path)
		all_paths_out_path_label.text = get_path_string_in_discord_format(currently_selected_path_node_indexes, currently_selected_links)
		call_resize()

func find_all_paths_out() -> void:
	all_paths_out.clear()
	selected_path_out_index = -1
	
	var from := all_paths_out_input.get_value()
	if from.is_empty(): return
	
	var zone_index := zone_names.find(from)
	if zone_index < 0 or zone_index >= zones.size(): return
	
	all_paths_out = graph.FindAllPathsOut(zone_index, all_paths_out_to_royal_toggle.button_pressed)
	if all_paths_out.size() > 0: selected_path_out_index = 0
	
