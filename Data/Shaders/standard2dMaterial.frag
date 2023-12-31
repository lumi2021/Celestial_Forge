#version 330 core

uniform int configDrawType;

in vec2 UV;

out vec4 out_color;

uniform vec4 backgroundColor;

void main()
{
    out_color = backgroundColor;
}