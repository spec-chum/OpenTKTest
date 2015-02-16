#version 330

layout(location = 0) in vec3 vertex_position;
layout(location = 1) in vec3 vertex_colour;
uniform float angle;

out vec3 colour;

void main()
{
	colour = vertex_colour;

	vec3 offset = vec3(cos(angle) * 0.5, sin(angle) * 0.5, 0.0);
	gl_Position = vec4(vertex_position + offset, 1.0);
}
