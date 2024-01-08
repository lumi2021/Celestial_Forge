#version 330 core

uniform int configDrawType;

in vec2 UV;

uniform vec4 color;
uniform sampler2D tex0;

out vec4 out_color;

void main()
{
    if (configDrawType == 0) // Solid color
        out_color = color;

    else if (configDrawType == 1) // Texture
        out_color = texture(tex0, UV);

    else if (configDrawType == 2) // Text
    {
        out_color.rgb = color.rgb;
        out_color.a = texture(tex0, UV).r * color.a;
    }
}