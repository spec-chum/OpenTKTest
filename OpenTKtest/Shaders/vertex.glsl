#version 330

layout(location = 0) in vec3 vertex_position;

uniform float angle;

void main()
{
	vec3 offset = vec3(cos(angle) * 0.5, sin(angle) * 0.5, 0.0);
	gl_Position = vec4(vertex_position + offset, 1.0);
}
