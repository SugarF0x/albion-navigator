extends Camera2D

@onready var background_shader_rect: ColorRect = %BackgroundShaderRect

@export var min_zoom := 0.5
@export var max_zoom := 4.0 

var is_dragging: bool = false
var is_mouse_in_scope: bool = false

func _ready() -> void:
	var center := get_viewport_rect().size / 2;
	position = center
	
	background_shader_rect.mouse_entered.connect(set_is_mouse_in_scope.bind(true))
	background_shader_rect.mouse_exited.connect(set_is_mouse_in_scope.bind(false))

func _input(event: InputEvent) -> void:
	if not is_mouse_in_scope: return
	
	if Input.is_action_just_pressed("zoom_in"): return adjust_zoom(true)
	if Input.is_action_just_pressed("zoom_out"): return adjust_zoom()
	
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT: 
			is_dragging = event.is_pressed()
			return
		
		if not event.is_pressed(): return
		if event.button_index == MOUSE_BUTTON_WHEEL_UP: adjust_zoom(true)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN: adjust_zoom()
		
		return
	
	if event is InputEventMouseMotion and is_dragging:
		position -= event.relative * (Vector2.ONE / zoom)
		return

func adjust_zoom(out := false) -> void:
	var delta := 0.1 if out else -0.1
	var new_zoom := zoom + Vector2(delta, delta)
	new_zoom.x = clamp(new_zoom.x, min_zoom, max_zoom)
	new_zoom.y = clamp(new_zoom.y, min_zoom, max_zoom)
	zoom = new_zoom

func set_is_mouse_in_scope(value: bool) -> void: 
	is_mouse_in_scope = value
	if not value: is_dragging = false
