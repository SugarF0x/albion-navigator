extends TabContainer

@onready var tab_size := get_tab_bar().size.y

func _ready() -> void:
	current_tab = -1
	tab_changed.connect(resize.call_deferred)
	get_viewport().size_changed.connect(resize.bind(-1).call_deferred)

var tween: Tween
func resize(_tab: int) -> void:
	await get_tree().process_frame
	
	var current_tab_content := get_current_tab_control()
	var tab_content_size := 0.0
	if current_tab_content:
		for child in current_tab_content.get_children():
			tab_content_size += child.size.y
	
	if tween != null: tween.stop()
	tween = create_tween()
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.set_ease(Tween.EASE_IN_OUT)
	
	tween.tween_method(tween_position.bind(get_viewport_rect().size.y - position.y, tab_content_size), 0.0, 1.0, 0.5)

func tween_position(percent: float, starting_size: float, new_tab_content_size: float) -> void:
	position.y = get_viewport_rect().size.y - lerpf(starting_size, new_tab_content_size + tab_size, percent)
	size.y = maxf(lerpf(starting_size, new_tab_content_size + tab_size, percent), new_tab_content_size + tab_size)
