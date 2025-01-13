extends VBoxContainer

const SAMPLE_IMAGE = preload("res://Assets/sample-image.png")

@onready var previous_button: Button = $HBoxContainer/PreviousButton
@onready var index_label: Label = $HBoxContainer/IndexLabel
@onready var next_button: Button = $HBoxContainer/NextButton
@onready var image_inspector: ImageInspector = $ImageInspector

var _captures: Array[Texture2D] = []
var _inspector_index := -1

func _ready() -> void:
	previous_button.pressed.connect(func() -> void: _inspector_index = maxi(_inspector_index - 1, 0); _sync_inspector())
	next_button.pressed.connect(func() -> void: _inspector_index = mini(_inspector_index + 1, _captures.size()); _sync_inspector())
	ScreenCapture.ScreenCaptured.connect(add_image)
	_sync_inspector()

func add_image(image: Texture2D) -> void:
	_captures.append(image)
	_sync_inspector()

func _sync_inspector() -> void:
	var total_captures := _captures.size()
	_inspector_index = mini(_inspector_index, total_captures - 1)
	if _inspector_index < 0 and total_captures > 0: _inspector_index = 0
	next_button.disabled = _inspector_index + 1 == total_captures
	previous_button.disabled = _inspector_index <= 0
	index_label.text = "-" if _inspector_index < 0 else str(_inspector_index + 1)
	image_inspector.texture = SAMPLE_IMAGE if _captures.size() <= 0 else _captures[_inspector_index]
