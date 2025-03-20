@tool
extends HFlowContainer

const ZONE_COMPONENT_VIEW = preload("res://Components/ZoneComponentView.tscn")

@export var components: Array[ZoneComponent] = [] :
	set(items):
		components = items
		sync_view()

func sync_view() -> void:
	for child in get_children(): child.queue_free()
	
	var component_count: Dictionary[int, int] = {}
	for component in components: 
		if component.Id not in component_count: component_count[component.Id] = 0
		component_count[component.Id] += 1
	
	for component_id in component_count:
		var view := ZONE_COMPONENT_VIEW.instantiate() as ZoneComponentView
		view.count = component_count[component_id]
		view.component = components[components.find_custom(func (c: ZoneComponent) -> bool: return c.Id == component_id)]
		add_child(view)
