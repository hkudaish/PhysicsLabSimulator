extends SceneTree

func _initialize() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://licenses"))
	var license_file = FileAccess.open("res://licenses/Godot-LICENSE.txt", FileAccess.WRITE)
	var attribution_file = FileAccess.open("res://licenses/Godot-THIRD-PARTY.json", FileAccess.WRITE)
	if license_file == null or attribution_file == null:
		push_error("Cannot write Godot attribution records.")
		quit(1)
		return
	license_file.store_string(Engine.get_license_text())
	attribution_file.store_string(JSON.stringify({
		"engine": Engine.get_version_info(),
		"copyrights": Engine.get_copyright_info(),
		"licenses": Engine.get_license_info()
	}, "  "))
	license_file.close()
	attribution_file.close()
	quit(0)
