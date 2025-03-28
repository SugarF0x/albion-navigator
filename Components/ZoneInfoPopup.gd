@tool
class_name ZoneInfoPopup extends PanelContainer

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

func fade(out := true) -> void:
	if not out: visible = true
	modulate = Color(1,1,1,1 if out else 0)
	
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(self, "modulate", Color(1,1,1,0 if out else 1), .5)
	if out: 
		await tween.finished
		visible = false
