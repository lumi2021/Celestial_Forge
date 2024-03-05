#version 330 core

uniform int configDrawType;

in vec2 UV;
in vec4 InstanceColor;

uniform vec4 color;
uniform sampler2D color_tex0;

out vec4 out_color;

void main()
{
    if (configDrawType == 0) // Solid color
        out_color = color;

    else if (configDrawType == 1) // Texture
        out_color = texture(color_tex0, UV);

    else if (configDrawType == 2) // Text
    {
        out_color.rgb = InstanceColor.rgb;
        out_color.a = texture(color_tex0, UV).r * InstanceColor.a;
    }
}