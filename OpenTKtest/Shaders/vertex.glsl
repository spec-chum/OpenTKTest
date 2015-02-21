#version 330

layout(location = 0) in vec3 vertPos;

uniform mat4 projMat;
uniform mat4 modelMat;
uniform float angle;

void main()
{
	vec3 offset = vec3(cos(angle) * 0.5, sin(angle) * 0.5, 0.0);
	gl_Position = projMat * modelMat * vec4(vertPos + offset, 1.0);
}
