#version 330 core

uniform int configDrawType;

in vec2 UV;

out vec4 out_color;

uniform vec4 color;

void main()
{
    out_color = vec4(UV.x, UV.y, 0.5, 1);//backgroundColor;
}