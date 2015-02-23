#version 330

const vec3 ambient = vec3(0.01, 0.01, 0.01);
const vec3 lightPosNormalised = normalize(vec3(-0.5, 0.2, 2.0));
const vec3 lightColour = vec3(1, 1, 1);

in vec3 normal;
in vec3 colour;

out vec4 frag_colour;

void main()
{
	float diffuse = clamp(dot(lightPosNormalised, normalize(normal)), 0.0, 1.0);
	frag_colour = vec4(ambient + colour * lightColour*diffuse, 1.0);
}