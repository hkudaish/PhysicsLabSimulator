shader_type spatial;
uniform vec4 surface_color : source_color = vec4(0.14, 0.185, 0.2, 1.0);
uniform float surface_roughness = 0.4;
varying vec3 world_position;

float random_value(vec2 point) {
    return fract(sin(dot(point, vec2(127.1, 311.7))) * 43758.5453);
}
float grain_noise(vec2 point) {
    vec2 cell = floor(point);
    vec2 fraction = fract(point);
    fraction = fraction * fraction * (3.0 - 2.0 * fraction);
    return mix(mix(random_value(cell), random_value(cell + vec2(1.0, 0.0)), fraction.x),
        mix(random_value(cell + vec2(0.0, 1.0)), random_value(cell + vec2(1.0, 1.0)), fraction.x), fraction.y);
}
void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}
void fragment() {
    vec2 position = world_position.xz * 65.0;
    float grain = grain_noise(position);
    float second = grain_noise(position * 1.73 + vec2(7.0, 19.0));
    float attenuation = 1.0 / (1.0 + dot(fwidth(position), fwidth(position)));
    ALBEDO = surface_color.rgb * (1.0 + (grain - 0.5) * 0.10 * attenuation);
    ROUGHNESS = clamp(surface_roughness + (second - 0.5) * 0.05, 0.0, 1.0);
    NORMAL_MAP = vec3(0.5 + (grain - 0.5) * 0.025 * attenuation,
        0.5 + (second - 0.5) * 0.025 * attenuation, 1.0);
    NORMAL_MAP_DEPTH = 0.2;
}
