#version 330 core

uniform int configDrawType;

in vec2 UV;

uniform vec4 color;
uniform sampler2D texture;

out vec4 out_color;

void main()
{
    if (configDrawType == 0) // Solid color
        out_color = color;

    else if (configDrawType == 1) // Texture
        out_color = texture(texture, UV);

    else if (configDrawType == 2) // Text
    {
        out_color.r = 1;
        out_color.g = 1;
        out_color.b = 1;
        out_color.a = texture(texture, UV).r;
    }
}