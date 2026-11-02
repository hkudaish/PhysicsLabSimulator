shader_type spatial;
render_mode blend_mix, depth_draw_never, cull_disabled;

// Thin, clear glass with Fresnel highlights. No guessed optical chemistry.
void fragment() {
    float grazing = pow(1.0 - clamp(abs(dot(normalize(NORMAL), normalize(VIEW))), 0.0, 1.0), 4.0);
    ALBEDO = vec3(0.80, 0.92, 0.95);
    METALLIC = 0.0;
    ROUGHNESS = 0.045;
    SPECULAR = 0.5;
    ALPHA = mix(0.018, 0.30, grazing);
}
