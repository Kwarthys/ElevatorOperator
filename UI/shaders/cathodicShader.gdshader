shader_type canvas_item;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable;

uniform float lowestCrt = 0.9;
uniform float crtFrequency = 250.0;
uniform float curvature = 5.0;

void fragment()
{
	vec2 uv = SCREEN_UV * 2.0 - 1.0;
	vec2 offset = abs(uv.gr) / vec2(curvature, curvature);
	uv = uv + uv * offset * offset;
	uv = uv * 0.5 + 0.5;
	
	float scanCoef = (0.5 * (1.0 + sin(uv.r * TAU * crtFrequency))) * (1.0-lowestCrt) + lowestCrt;
	
	if(uv.r < 0.0 || uv.r > 1.0 || uv.g < 0.0 || uv.g > 1.0)
		COLOR = vec4(0.1,0.1,0.1,1.0);
	else
	{
		COLOR = textureLod(screen_texture, uv, 0.0);
		COLOR.rgb *= scanCoef * scanCoef;
	}
	//float debug = scanCoef;
	//COLOR = vec4(debug, debug, debug, 1.0);
}
