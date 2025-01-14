@tool
class_name ImageInspector extends PanelContainer

@onready var texture_rect: TextureRect = $TextureRect

@export var texture: Texture2D : 
	set(value):
		texture = value
		_sync_texture()

var is_dragging: bool = false
var is_mouse_in_scope: bool = false

const min_scale := 1.0
const max_scale := 8.0

func _ready() -> void:
	_sync_texture()
	
	if Engine.is_editor_hint(): return
	mouse_entered.connect(set_is_mouse_in_scope.bind(true))
	mouse_exited.connect(set_is_mouse_in_scope.bind(false))

func _input(event: InputEvent) -> void:
	if not is_mouse_in_scope: return
	
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT: 
			is_dragging = event.is_pressed()
			return
		
		if not event.is_pressed(): return
		if event.button_index == MOUSE_BUTTON_WHEEL_UP: adjust_scale(true)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN: adjust_scale()
	
	elif event is InputEventMouseMotion and is_dragging:
		adjust_position(texture_rect.position + event.relative)

func adjust_scale(out := false) -> void:
	var delta := 0.1 if out else -0.1
	var new_scale := texture_rect.scale + Vector2(delta, delta)
	new_scale.x = clamp(new_scale.x, min_scale, max_scale)
	new_scale.y = clamp(new_scale.y, min_scale, max_scale)
	
	var mouse_pre_scale := texture_rect.get_local_mouse_position()
	texture_rect.scale = new_scale
	var mouse_post_scale := texture_rect.get_local_mouse_position()
	
	adjust_position(texture_rect.position + (mouse_post_scale - mouse_pre_scale) * new_scale.x)

func adjust_position(new_position := texture_rect.position) -> void:
	new_position.x = clampf(new_position.x, size.x - size.x * texture_rect.scale.x, 0)
	new_position.y = clampf(new_position.y, size.y - size.y * texture_rect.scale.y, 0)
	texture_rect.position = new_position

func set_is_mouse_in_scope(value: bool) -> void: 
	is_mouse_in_scope = value
	if not value: is_dragging = false

func _sync_texture() -> void:
	if not texture_rect: return
	texture_rect.texture = texture
