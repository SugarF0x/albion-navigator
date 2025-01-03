shader_type canvas_item;

// Colors
uniform vec4 base_color : source_color = vec4(0.2, 0.23, 0.27, 1.0); // Base color ($c)
uniform vec4 grid_color : source_color = vec4(1.0, 1.0, 1.0, 0.2);   // Transparent white ($b)
uniform vec4 cell_color : source_color = vec4(1.0, 1.0, 1.0, 0.1);   // Transparent white ($t)
uniform vec4 transparent_color : source_color = vec4(0.0, 0.0, 0.0, 0.0);   // Fully transparent ($n)

// Grid settings
uniform float cell_size = 25.0; // Size of the grid
uniform float grid_multiplier = 5.0;

uniform float cell_thickness = 1.0;
uniform float grid_thickness = 2.0;
uniform float threshold_divider = 1.9;

void fragment() {
	vec2 uv = FRAGCOORD.xy / SCREEN_PIXEL_SIZE;
	
	COLOR = base_color;
	
	bool is_on_cell_border = (
		mod(FRAGCOORD.x, cell_size) < cell_thickness / threshold_divider
		|| mod(FRAGCOORD.x, cell_size) > cell_size - cell_thickness / threshold_divider
		|| mod(FRAGCOORD.y, cell_size) < cell_thickness / threshold_divider
		|| mod(FRAGCOORD.y, cell_size) > cell_size - cell_thickness / threshold_divider
	);
	
	float grid_size = cell_size * grid_multiplier;
	bool is_on_grid_border = (
		mod(FRAGCOORD.x, grid_size) < grid_thickness / threshold_divider
		|| mod(FRAGCOORD.x, grid_size) > grid_size - grid_thickness / threshold_divider
		|| mod(FRAGCOORD.y, grid_size) < grid_thickness / threshold_divider
		|| mod(FRAGCOORD.y, grid_size) > grid_size - grid_thickness / threshold_divider
	);
	
	if (is_on_grid_border) {
		COLOR = grid_color;
	} else if (is_on_cell_border) {
		COLOR = cell_color;
	}
	
	float darkness_factor = clamp(UV.y - 0.60, 0.0, 0.40);
	COLOR = vec4(COLOR.rgb * (1.0 - darkness_factor), COLOR.a);
}
