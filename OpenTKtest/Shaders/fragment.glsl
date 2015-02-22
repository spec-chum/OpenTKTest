#version 330

const vec3 ambient = vec3(0.2, 0.2, 0.2);
const vec3 lightPosNormalised = normalize(vec3(-0.5, 0.5, 2.0));
const vec3 lightColor = vec3(0.7, 0.7, 0.7);

in vec3 normal;

out vec4 frag_colour;

void main()
{
	float diffuse = clamp(dot(lightPosNormalised, normalize(normal)), 0.0, 1.0);
	frag_colour = vec4(ambient + diffuse * lightColor, 1.0);
}