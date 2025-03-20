extends TabBar

@onready var zone_line: AutoCompleteLineEdit = %ZoneLine
@onready var search_button: Button = %SearchButton
@onready var id_label: Label = %IdLabel
@onready var type_label: Label = %TypeLabel
@onready var layer_label: Label = %LayerLabel
@onready var display_name_label: Label = %DisplayNameLabel
@onready var connections_label: Label = %ConnectionsLabel
@onready var zone_component_stack: HFlowContainer = %ZoneComponentStack

const ZONE_COMPONENT_VIEW = preload("res://Components/ZoneComponentView.tscn")
const ZONE_GROUP := preload('res://Resources/ZoneGroup.tres')

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

enum ZoneLayer {
	NonApplicable,
	L1Royal,
	L1RoyalRed,
	L1Outer,
	L1Middle,
	L1Inner,
	L2Outer,
	L3Hub,
	L2Middle,
	L2Inner,
	L3Deep,
	L2Rest,
	L3DeepRest,
}

var zones: Array[Zone] = []
var zone_names: Array[String] = []

func _ready() -> void:
	for zone in ZONE_GROUP.load_all():
		zones.append(zone as Zone)
		zone_names.append(zone.DisplayName)
	
	zone_line.options = zone_names
	search_button.pressed.connect(on_search)

func _input(_event: InputEvent) -> void:
	if not Input.is_action_just_pressed("ui_text_submit"): return
	if not zone_line.has_focus(): return
	accept_event()
	on_search()

func on_search() -> void:
	var search_value := zone_line.get_value()
	if search_value.is_empty(): return
	
	show_zone_details(search_value)
	
	zone_line.text = ""
	zone_line.text_changed.emit("")
	zone_line.grab_focus.call_deferred()
	
	# hacky solution to update tab size but it works (disgusting)
	(get_parent_control() as TabContainer).tab_changed.emit(-1)

func show_zone_details(zone_name: String) -> void:
	var zone_index := zone_names.find(zone_name)
	if zone_index < 0: return
	
	var zone := zones[zone_index]
	id_label.text = "Id: {id}".format({ "id": zone.Id })
	type_label.text = "Type: {type}".format({ "type": ZoneType.keys()[zone.Type] })
	layer_label.text = "Layer: {layer}".format({ "layer": ZoneLayer.keys()[zone.Layer] })
	display_name_label.text = "Display Name: {display_name}".format({ "display_name": zone.DisplayName })
	connections_label.text = "Connections: {connections}".format({ "connections": ", ".join(zone.Connections.map(func (connection_index: int) -> String: return zone_names[connection_index])) })
	zone_component_stack.components = zone.Components
