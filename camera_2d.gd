extends Camera2D

@export var min_zoom := 0.5
@export var max_zoom := 4.0 

var is_dragging: bool = false

func _ready() -> void:
	var center := get_viewport_rect().size / 2;
	position = center

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT: is_dragging = event.is_pressed()
		elif event.button_index == MOUSE_BUTTON_WHEEL_UP: adjust_zoom(true)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN: adjust_zoom()
	
	elif event is InputEventMouseMotion and is_dragging:
		position -= event.relative * (Vector2.ONE / zoom)

func adjust_zoom(out := false) -> void:
	var delta := 0.1 if out else -0.1
	var new_zoom := zoom + Vector2(delta, delta)
	new_zoom.x = clamp(new_zoom.x, min_zoom, max_zoom)
	new_zoom.y = clamp(new_zoom.y, min_zoom, max_zoom)
	zoom = new_zoom
