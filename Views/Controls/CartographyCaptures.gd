class_name CartographyCaptures extends VBoxContainer

const SAMPLE_IMAGE = preload("res://Assets/sample-image.png")

@onready var previous_button: Button = $HBoxContainer/PreviousButton
@onready var index_label: Label = $HBoxContainer/IndexLabel
@onready var next_button: Button = $HBoxContainer/NextButton
@onready var image_inspector_1: ImageInspector = $HBoxContainer2/ImageInspector1
@onready var image_inspector_2: ImageInspector = $HBoxContainer2/ImageInspector2
@onready var clear_button: Button = $HBoxContainer/ClearButton

var _captures: Array[Texture2D] = []
var _capture_stamps: Array[String] = []
var _inspector_index := -1

signal preview_image_changed(index: int, image: Texture2D, timestamp: String)

func _ready() -> void:
	previous_button.pressed.connect(func() -> void: _inspector_index = maxi(_inspector_index - 1, 0); _sync_inspector())
	next_button.pressed.connect(func() -> void: _inspector_index = mini(_inspector_index + 1, _captures.size()); _sync_inspector())
	clear_button.pressed.connect(pop_current_image)
	ScreenCapture.ScreenCaptured.connect(add_image)
	_sync_inspector()

func add_image(image: Texture2D) -> void:
	_captures.append(image)
	_capture_stamps.append(Time.get_datetime_string_from_system(true))
	_sync_inspector()

func pop_current_image() -> void:
	if _inspector_index < 0 or _inspector_index > _captures.size() - 1: return
	_captures.remove_at(_inspector_index)
	_capture_stamps.remove_at(_inspector_index)
	_sync_inspector()

func get_current_expiration() -> String:
	return _capture_stamps[_inspector_index] if _capture_stamps.size() - 1 >= _inspector_index and _inspector_index >= 0 else ""

func _sync_inspector() -> void:
	var total_captures := _captures.size()
	
	clear_button.disabled = total_captures == 0
	
	_inspector_index = mini(_inspector_index, total_captures - 1)
	if _inspector_index < 0 and total_captures > 0: _inspector_index = 0
	
	next_button.disabled = _inspector_index + 1 == total_captures
	previous_button.disabled = _inspector_index <= 0
	
	index_label.text = "-" if _inspector_index < 0 else str(_inspector_index + 1)
	
	var texture := SAMPLE_IMAGE if total_captures <= 0 else _captures[_inspector_index]
	image_inspector_1.texture = texture
	image_inspector_2.texture = texture
	
	if total_captures == 0:
		image_inspector_1.texture_rect.scale = Vector2.ONE
		image_inspector_1.adjust_scale()
		image_inspector_2.texture_rect.scale = Vector2.ONE
		image_inspector_2.adjust_scale()
	
	preview_image_changed.emit(_inspector_index, texture, get_current_expiration())
