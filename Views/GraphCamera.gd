class_name GraphCamera extends Camera2D

@onready var background_shader_rect: ColorRect = %BackgroundShaderRect

@export var min_zoom := 0.5
@export var max_zoom := 8.0
@export var zone_map: ZoneMap 

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

func set_zoom_level(value: float) -> void:
	var new_zoom := Vector2(value, value)
	new_zoom.x = clamp(new_zoom.x, min_zoom, max_zoom)
	new_zoom.y = clamp(new_zoom.y, min_zoom, max_zoom)
	zoom = new_zoom
	
	adjust_zone_label_visibility()

func adjust_zoom(out := false) -> void:
	var delta := 0.1 if out else -0.1
	var new_zoom := zoom.x + delta
	set_zoom_level(new_zoom)

var are_names_hidden := true
func adjust_zone_label_visibility() -> void:
	var should_hide := zoom.x < 1.0
	if should_hide == are_names_hidden: return
	are_names_hidden = should_hide
	
	for node: ForceGraphNode in zone_map.Nodes:
		if node is ZoneNode:
			if should_hide: node.HideName()
			else: node.ShowName()

func set_is_mouse_in_scope(value: bool) -> void: 
	is_mouse_in_scope = value
	if not value: is_dragging = false

func transition_to_position(new_pos: Vector2, new_zoom: float) -> void:
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.set_ease(Tween.EASE_IN_OUT)
	
	tween.tween_method(tween_position.bind(position, zoom.x, new_pos, new_zoom), 0.0, 1.0, .75)

func tween_position(percent: float, start_pos: Vector2, start_zoom: float, end_pos: Vector2, end_zoom: float) -> void:
	position = start_pos.lerp(end_pos, percent)
	var zoom_value := lerpf(start_zoom, end_zoom, percent)
	zoom = Vector2(zoom_value, zoom_value)
	adjust_zone_label_visibility()
