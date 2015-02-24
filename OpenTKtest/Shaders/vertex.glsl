#version 330

layout(location = 0) in vec3 vertPos;
layout(location = 1) in vec3 colours;
layout(location = 2) in vec3 normals;

uniform mat4 MVP;
uniform mat4 modelMat;
uniform float angle;

out vec3 normal;
out vec3 colour;

void main()
{
	normal = (modelMat * vec4(normals, 0)).xyz;
	colour = colours;

	vec3 offset = vec3(cos(angle) * 0.5, sin(angle) * 0.5, 0.0);
	gl_Position = MVP * vec4(vertPos + offset, 1.0);
}
