@tool
extends Node

@export var resource: Resource
@export_tool_button('Reimport') var reimport := reimport_data

func reimport_data() -> void:
	if resource.new() is not ZoneComponent: return
	
	var save_path := "res://Resources/ZoneComponents/"
	var data_file := FileAccess.open("res://Resources/ZoneComponents/zoneComponentsData.json", FileAccess.READ)
	var json_data: Array = JSON.parse_string(data_file.get_as_text())
	
	var dir := DirAccess.open(save_path)
	for file in dir.get_files():
		if file.contains(".tres"): dir.remove(save_path + file)
	
	var max_id := 0
	for entry: Dictionary in json_data:
		max_id = maxi(int(entry["Id"]), max_id)
	
	for entry: Dictionary in json_data:
		var res: ZoneComponent = resource.new()
		
		res.Id = int(entry["Id"])
		res.Type = int(entry["Type"])
		res.DisplayName = entry["DisplayName"]
		res.Tier = int(entry["Tier"])
		res.Properties = (entry["Properties"] as Array).map(func (item: float) -> int: return int(item))
		
		var file_name := str(res.Id).pad_zeros(str(max_id).length())
		ResourceSaver.save(res, save_path + file_name + ".tres")
