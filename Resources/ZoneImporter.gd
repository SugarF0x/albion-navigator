@tool
extends Node

const ZONE_COMPONENTS_GROUP = preload("res://Resources/ZoneComponentsGroup.tres")

@export var resource: Resource
@export_tool_button('Reimport') var reimport := reimport_data

func reimport_data() -> void:
	if resource.new() is not ZoneResource: return
	
	var save_path := "res://Resources/Zones/"
	var data_file := FileAccess.open("res://Resources/zoneData.json", FileAccess.READ)
	var json_data: Array = JSON.parse_string(data_file.get_as_text())
	
	var dir := DirAccess.open(save_path)
	for file in dir.get_files():
		if file.contains(".tres"): dir.remove(save_path + file)
	
	var components_resource_map := ZONE_COMPONENTS_GROUP.load_all()
	
	for entry: Dictionary in json_data:
		var res: ZoneResource = resource.new()
		
		res.Id = int(entry["id"])
		res.Type = int(entry["type"])
		res.Layer = int(entry["layer"])
		res.DisplayName = entry["displayName"]
		var posArray: Array = (entry["position"] as Array).map(func (pos: float) -> float: return float(pos))
		res.Position = Vector2(posArray[0], posArray[1])
		res.Connections = (entry["connections"] as Array).map(func (item: float) -> int: return int(item))
		res.Components = (entry['components'] as Array).map(func (id: int) -> Resource: return components_resource_map[id])
		
		var file_name := res.DisplayName.replace(" ", "_")
		var resource_save_path := save_path + file_name + ".tres"
		ResourceSaver.save(res, resource_save_path)
