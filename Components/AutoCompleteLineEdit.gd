class_name AutoCompleteLineEdit extends LineEdit

@onready var label: Label = $MarginContainer/Label

@export var options: Array[String] = []

var valid_options: Array[String] = []
var option_hint_index := -1 :
	set(value):
		option_hint_index = value
		_sync_hint_index_to_label()

func _ready() -> void:
	_sync_hint_index_to_label()
	
	text_changed.connect(on_text_change)

func _input(_event: InputEvent) -> void:
	if not has_focus(): return
	if option_hint_index < 0: return
	if Input.is_action_just_pressed("ui_down"): option_hint_index = clampi(option_hint_index + 1, 0, valid_options.size() - 1); accept_event()
	if Input.is_action_just_pressed("ui_up"): option_hint_index = clampi(option_hint_index - 1, 0, valid_options.size() - 1); accept_event()

func on_text_change(new_text: String) -> void:
	var previous_valid_options_size := valid_options.size()
	if new_text == "": valid_options.clear()
	else: valid_options = options.filter(func (word: String) -> bool: return word.to_lower().begins_with(new_text.to_lower()))
	
	var new_valid_options_Size := valid_options.size()
	if previous_valid_options_size == new_valid_options_Size: return
	option_hint_index = -1 if valid_options.size() == 0 else 0

func get_value() -> String:
	if option_hint_index < 0: return ""
	return valid_options[option_hint_index]

func _sync_hint_index_to_label() -> void:
	if not label: return
	if option_hint_index < 0: label.text = ""
	else: label.text = valid_options[option_hint_index]
