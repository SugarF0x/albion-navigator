extends TabBar

const LinkScene := preload("res://Entities/ZoneLink.tscn")

@onready var graph := get_tree().get_first_node_in_group("ForceGraph") as ZoneMap
@onready var register_button: Button = $MarginContainer/HBoxContainer/VBoxContainer/RegisterButton
@onready var source_zone_edit: AutoCompleteLineEdit = $MarginContainer/HBoxContainer/VBoxContainer/SourceZoneEdit
@onready var target_zone_edit: AutoCompleteLineEdit = $MarginContainer/HBoxContainer/VBoxContainer/TargetZoneEdit
@onready var cartography_captures: CartographyCaptures = $MarginContainer/HBoxContainer/CartographyCaptures
@onready var captured_at_label: Label = $MarginContainer/HBoxContainer/VBoxContainer/CapturedAtLabel
@onready var hours_edit: LineEdit = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/HoursEdit
@onready var minutes_edit: LineEdit = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/MinutesEdit
@onready var seconds_edit: LineEdit = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/SecondsEdit

func _ready() -> void:
	graph.ChildrenRegistered.connect(register_zone_names)
	register_button.pressed.connect(register_new_link)
	cartography_captures.preview_image_changed.connect(on_preview_image_changed)

func _input(_event: InputEvent) -> void:
	if not Input.is_action_just_pressed("ui_text_submit"): return
	
	var controls: Array[Control] = [source_zone_edit, target_zone_edit, hours_edit, minutes_edit, seconds_edit]
	if controls.all(func(node: Control) -> bool: return not node.has_focus()): return
	
	register_new_link()

func register_zone_names(nodes: Array, _links: Array) -> void:
	var zone_names: Array[String]
	for node: ForceGraphNode in nodes:
		if node is not ZoneNode: continue
		if node.DisplayName == "": continue
		zone_names.append(node.DisplayName)
	
	source_zone_edit.options = zone_names
	target_zone_edit.options = zone_names

func register_new_link() -> void:
	var source_text := source_zone_edit.get_value()
	var target_text := target_zone_edit.get_value()
	if source_text == "" or target_text == "": return
	
	var nodes := graph.Nodes as Array
	var source: int = -1
	var target: int = -1
	
	for node: ZoneNode in nodes:
		if node.DisplayName == source_text: source = node.Index
		if node.DisplayName == target_text: target = node.Index
		if source > 0 and target > 0: break
	
	if source < 0 or target < 0: return
	
	graph.AddPortal(source, target, get_input_expiration())
	
	target_zone_edit.text = ""
	target_zone_edit.text_changed.emit("")
	hours_edit.text = ""
	minutes_edit.text = ""
	seconds_edit.text = ""
	
	source_zone_edit.grab_focus()
	cartography_captures.pop_current_image()

func get_input_expiration() -> String:
	var time := Time.get_unix_time_from_datetime_string(current_capture_timestamp)
	
	var additional_time := 0
	additional_time += int(hours_edit.text) * 3600
	additional_time += int(minutes_edit.text) * 60
	additional_time += int(seconds_edit.text)
	
	time += additional_time
	return Time.get_datetime_string_from_unix_time(time)

@onready var base_captured_at_text := captured_at_label.text
var current_capture_timestamp := "" :
	set(value):
		current_capture_timestamp = value
		captured_at_label.text = base_captured_at_text + value
	get():
		return current_capture_timestamp if current_capture_timestamp != "" else Time.get_datetime_string_from_system(true)

func on_preview_image_changed(_index: int, _image: Texture2D, timestamp: String) -> void:
	current_capture_timestamp = timestamp
