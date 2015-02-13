#version 410

in vec3 vp;

//  test

void main()
{
	gl_Position = vec4 (vp, 1.0);
}