extends TabContainer

@onready var tab_size := get_tab_bar().size.y

func _ready() -> void:
	current_tab = -1
	tab_changed.connect(resize)

func resize(_tab: int) -> void:
	var current_tab_content := get_current_tab_control()
	var tab_content_size := 0.0
	if current_tab_content:
		for child in current_tab_content.get_children(): tab_content_size += child.size.y
	
	size.y = get_viewport_rect().size.y
	var tween := create_tween()
	await tween.tween_property(self, "position", Vector2(0, size.y - tab_content_size - tab_size), 0.5) \
		.set_trans(Tween.TRANS_CUBIC) \
		.set_ease(Tween.EASE_IN_OUT) \
		.finished
	size.y = tab_content_size + tab_size

func open() -> void:
	var tween := create_tween()
	tween.tween_property(self, "position", Vector2.ZERO, 0.5)

func close() -> void:
	var tween := create_tween()
	tween.tween_property(self, "position", Vector2.ZERO, 0.5)
