@tool
extends PanelContainer

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

@onready var name_label: Label = %NameLabel
@onready var layer_label: Label = %LayerLabel
@onready var zone_component_stack: HFlowContainer = %ZoneComponentStack

@export var zone: Zone :
	set(value):
		zone = value
		if not value: return
		if not is_node_ready(): await ready
		
		name_label.text = value.DisplayName
		layer_label.text = ZoneLayer.keys()[value.Layer]
		layer_label.visible = value.Layer > 0
		zone_component_stack.components = value.Components
